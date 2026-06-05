using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Gear;
using Project1.Core.Resources;
using Project1.Core.Systems.Crafting;
using Project1.Core.Systems.Recipes;
using System.Data;
using System.Linq;
using static Project1.Core.Systems.Crafting.CraftingOrder;

namespace Project1.Core.AI.Planners;

sealed class Planner_Crafting : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        var map = actor.Map;
        var carried = actor.Hauled;
        var manager = map.Town.Crafting;

        if (manager.TryGetCommitedOrder(actor, out var existing))
        {
            var (flowControl, value) = tryOrder(actor, map, carried, manager, existing);
            if (flowControl)
                return value;
            else
                manager.Uncommit(actor);
        }

        // Gather all pending, reachable orders
        // Exclude unreachable/unreservable workstations early instead of performing the check for each workstation order
        var allOrders = manager.AllWorkstations
            .Where(comp => actor.CanReachAndReserve(comp.Parent))
            .SelectMany(comp => comp.Orders)
            .Where(o => o.Pending);

        // Guard: don't interfere if carrying irrelevant item
        // BUT when implementing repair orders, the actor will be carrying a repairable object
        // so let's try removing this guard and hope ;)
        //if (carried != null && !allOrders.Any(o => o.Matches(carried)))
        //    return null;

        foreach (var order in allOrders)
        {
            (bool flowControl, Plan value) = tryOrder(actor, map, carried, manager, order);
            switch (flowControl)
            {
                case false: continue;
                case true: return value;
            }
        }

        return null;
    }

    private static (bool flowControl, Plan value) tryOrder(Actor actor, Simulation.MapBase map, Entity carried, CraftingManager manager, CraftingOrder order)
    {
        if (order.ProductDef is Def recipe && actor.Recipes.Get(recipe) < order.MinMastery)
            return (false, null);

        if (manager.ProductToMove(actor) is Entity toMove)
        {
            // haul it explicitly to the output stockpile of the order? or let other planners claim it?
            if (carried == toMove)
                return (false, null);
            if (actor.CanReachAndReserve(toMove))
            {
                manager.Uncommit(actor);
                return (true, new Plan(PlanDefOf.GoHaul, toMove) { Continuation = PlanContinuationPolicy.Yield });
            }
        }

        // Check if another actor is currently commited to this order
        if (!manager.CanCommit(actor, order))
            return (flowControl: false, value: null);

        if (TryUnfinishedItem(actor, order) is Plan unfinishedPlan)
        {
            return (flowControl: true, value: unfinishedPlan);
        }

        if (TryRepairPlan(actor, order) is Plan repairPlan)
        {
            return (flowControl: true, value: repairPlan);
        }
        // Check fuel requirement
        if (!order.CheckFuelReq())
            return (flowControl: false, value: null);

        //if(!actor.HasDuty(order.WorkstationCapability.))

        // Track slots we want to exclude (already being deposited into by others)
        var workstationSlots = order.Workstation.Parent.CellsOccupied;
        var excludedSlots = workstationSlots.Select(c => c.Above).Where(c => !actor.CanReachAndReserve(c)).ToHashSet();
        if (excludedSlots.Count == workstationSlots.Count)
            return (flowControl: false, value: null);

        var candidateIngredients = map.Hauling.GetItems(order.Workstation.Input).Where(actor.CanReachAndReserve);

        if (carried is not null)
            candidateIngredients = candidateIngredients.Prepend(carried);
        var candidates = candidateIngredients.ToList();

        // Evaluate feasibility with exclusions
        var feasibility = order.IsFeasibleNew(candidates, excludedSlots, carried);

        if (feasibility.State == CraftingOrderState.NotEnoughItems)
            return (flowControl: false, value: null);

        manager.Commit(order, actor);

        // All slots already satisfied
        if (feasibility.State == CraftingOrderState.ReadyToCraft &&
            carried == null &&
            actor.CanReachAndReserve(order.Workstation.Parent))
        {
            var withUnfinishedItem = order.WorkstationCapability.Worker.CreatesUnfinished;
            var plandef = withUnfinishedItem ? PlanDefOf.CraftingUnfinishedBegin : PlanDefOf.Crafting;
            var plan = new Plan(plandef, new InteractionTarget(map, order.Workstation.Parent.OriginGlobal))
            {
                Order = order.Id,
                TargetB = new InteractionTarget(order.Workstation.Parent)
            };

            foreach (var allocation in feasibility.ArmedSlots)
                plan.AddTarget(TargetIndex.A, allocation.Entity);
            //manager.Commit(actor, order.Workstation, order, feasibility.ArmedSlots.Select(i => i.Entity));
            return (flowControl: true, value: plan);
        }

        // Carried item can be deposited
        if (carried is not null)
        {
            var carriedAlloc = feasibility.Allocations
                .FirstOrDefault(a => a.Entity == carried);
            if (carriedAlloc.Entity is not null)
            {
                manager.BindIngredient(actor, order, carried, carriedAlloc.Bone);

                if (carried.StackSize >= carriedAlloc.Quantity)
                {
                    // Fully satisfies → deposit now
                    return (flowControl: true, value: new Plan(PlanDefOf.GoPlace,
                        new InteractionTarget(map, carriedAlloc.Slot))
                    {
                        AmountA = carriedAlloc.Quantity,
                        TargetB = new InteractionTarget(order.Workstation.Parent)
                    });
                }
                else
                {
                    // Partially satisfies → gather remainder first
                    var remainderAlloc = feasibility.Allocations
                        .First(a => a.Slot == carriedAlloc.Slot && a.Entity != carried);

                    return (flowControl: true, value: new Plan(PlanDefOf.GoHaul,
                        new InteractionTarget(remainderAlloc.Entity))
                    {
                        AmountA = remainderAlloc.Quantity,
                        TargetB = new InteractionTarget(order.Workstation.Parent)
                    });
                }
            }
        }

        // Otherwise, pick up next needed item
        if (carried is not null)
        {
            var correctAlloc = feasibility.Allocations.FirstOrDefault(a => carried.CanAbsorb(a.Entity));
            if (correctAlloc.Entity != null)
            {
                return (flowControl: true, value: new Plan(PlanDefOf.GoHaul, new InteractionTarget(correctAlloc.Entity))
                {
                    AmountA = correctAlloc.Quantity,
                    TargetB = new InteractionTarget(order.Workstation.Parent)
                });
            }
            else return (flowControl: true, value: null); // the carried item it irrelevant to crafting, to return null so fallback planners can handle it
        }
        //foreach (var alloc in feasibility.Allocations)
        //{
        //    return new Plan(PlanDefOf.GoHaul, new TargetArgs(alloc.Entity))
        //    {
        //        AmountA = alloc.Quantity
        //    };
        //}

        // Consolidate allocations if they're from the same stack
        var targetAlloc = feasibility.Allocations.First();
        var targetStack = targetAlloc.Entity;
        int totalQuantity = 0;
        foreach (var alloc in feasibility.Allocations)
            if (alloc.Entity == targetStack)
                totalQuantity += alloc.Quantity;

        // commit this stack to the commitment's ingredients

        return (flowControl: true, value: new Plan(PlanDefOf.GoHaul, new InteractionTarget(targetStack))
        {
            AmountA = totalQuantity,
            TargetB = new InteractionTarget(order.Workstation.Parent)
        });
        // Otherwise, pick up next needed item
        //foreach (var alloc in feasibility.Allocations)
        //{
        //    // Skip entities already carried
        //    if (alloc.Entity == carried)
        //        continue;

        //    return new Plan(PlanDefOf.GoHaul, new TargetArgs(alloc.Entity))
        //    {
        //        AmountA = alloc.Quantity
        //    };
        //}
    }

    private static Plan TryUnfinishedItem(Actor actor, CraftingOrder order)
    {
        if (order.UnfinishedItem is not Entity unfinishedItem)
            return null;
        //var comp = unfinishedItem.GetComponent<UnfinishedItemComp>();
        //if (comp.Author != actor)
        if (unfinishedItem.Author != actor)
            return null;
        if (!actor.CanReachAndReserve(unfinishedItem))
            return null;
        var map = actor.Map;
        var cell = unfinishedItem.Cell;
        var workstation = order.Workstation.Parent;
        if (cell.Below == workstation.OriginGlobal)
        {
            return new Plan(PlanDefOf.CraftingUnfinishedAdvance, new InteractionTarget(map, workstation.OriginGlobal))
            {
                TargetB = new InteractionTarget(workstation),
                Order = order.Id
            };
        }
        if (actor.Hauled is Entity carried && carried == order.UnfinishedItem)
        {
            if (actor.CanReachAndReserve(workstation) && map.IsCellEmpty(workstation.OriginGlobal.Above))
            {
                return new Plan(PlanDefOf.GoPlace, new InteractionTarget(map, workstation.OriginGlobal.Above))
                {
                    TargetB = new InteractionTarget(workstation)
                };
            }
            else
                return null;
        }
        if (actor.CanReachAndReserve(unfinishedItem))
        {
            return new Plan(PlanDefOf.GoHaul, unfinishedItem);
        }
        return null;
    }
    private static Plan TryRepairPlan(Actor actor, CraftingOrder order)
    {
        if (order.WorkstationCapability != WorkstationCapabilityDefOf.Repairing)
            return null;

        var map = actor.Map;
        var workstation = order.Workstation;
        var benchCell = workstation.Parent.OriginGlobal;

        var itemsOnBench = map.GetEntitiesAt(benchCell.Above);
        if (itemsOnBench.FirstOrDefault(isRepairable) is Entity repairableItem)
            return new Plan(PlanDefOf.Repairing, repairableItem) { TargetB = new InteractionTarget(map, workstation.Parent.OriginGlobal), TargetC = new InteractionTarget(workstation.Parent) };

        if (actor.Hauled is Entity hauled && isRepairable(hauled))
            return new Plan(PlanDefOf.GoPlace, new InteractionTarget(map, benchCell.Above)) { TargetB = new InteractionTarget(workstation.Parent) };

        if (actor.Gear[GearSlotDefOf.Mainhand] is Entity repairableGear && isRepairable(repairableGear))
            return new Plan(PlanDefOf.Unequip, repairableGear);

        if (actor.Inventory.Contents.FirstOrDefault(isRepairable) is Entity repairableInvItem)
            return new Plan(PlanDefOf.RetrieveFromInventory, repairableInvItem);

        if (map.Hauling.GetItems(order.Workstation.Input).FirstOrDefault(isRepairable) is Entity repairableStockpileItem)
            return new Plan(PlanDefOf.GoHaul, repairableStockpileItem);

        return null;

        static bool isRepairable(Entity e) => e.Resources?.View(ResourceDefOf.Durability) is IResourceView durability && durability.Percentage < 1;
    }
}
