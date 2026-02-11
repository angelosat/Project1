using System.Linq;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Labors;
using Project1.Core.Entities;
using Project1.Core.Gear;
using Project1.Core.Towns;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Crafting;
using static Project1.Core.Crafting.CraftingOrder;

namespace Project1.Core.AI.Planners
{
    class PlannerCrafting : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Craftsman))
                return null;

            var map = actor.Map;
            var carried = actor.Hauled as Entity;
            var manager = map.Town.CraftingManagerNew;

            // Gather all pending, reachable orders
            //var allOrders = manager.GetAllOrdersUnsorted()
            //    .Where(o => o.Pending && actor.CanReachAndReserve(o.Workstation.Parent));

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
                if(TryRepairPlan(actor, order) is Plan repairPlan)
                {
                    return repairPlan;
                }
                // Check fuel requirement
                if (!order.CheckFuelReq())
                    continue;

                // Track slots we want to exclude (already being deposited into by others)
                var workstationSlots = order.Workstation.Parent.CellsOccupied;
                var excludedSlots = workstationSlots.Select(c => c.Above).Where(c => !actor.CanReachAndReserve(c)).ToHashSet();
                if (excludedSlots.Count == workstationSlots.Count)
                    continue;

                //IEnumerable<Entity> itemPool =
                //    order.Workstation.Input != ZoneId.Null ?
                //    map.Town.ZoneManager.GetZone<Stockpile>(order.Workstation.Input).Items :
                //    map.Stockpiles.Items;

                var candidateIngredients = map.Stockpiles.GetItems(order.Workstation.Input).Where(i => i.Def == ItemDefOf.Ingredient && actor.CanReachAndReserve(i));
                //var candidateIngredients = map.GetEntities<Entity>().Where(i=>i.Def == ItemDefOf.Ingredient && actor.CanReachAndReserve(i));

                if (carried is not null)
                    candidateIngredients = candidateIngredients.Prepend(carried);
                var candidates = candidateIngredients.ToList();

                // Build candidate pool (carried first if exists)
                //var candidates = carried != null
                //    ? new List<Entity> { carried }.Concat(map.GetEntities<Entity>().Where(actor.CanReachAndReserve)).ToList()
                //    : map.GetEntities<Entity>().Where(actor.CanReachAndReserve).ToList();


                // Evaluate feasibility with exclusions
                var feasibility = order.IsFeasibleNew(candidates, excludedSlots, carried);

                if (feasibility.State == CraftingOrderState.NotEnoughItems)
                    continue;

                // All slots already satisfied
                if (feasibility.State == CraftingOrderState.ReadyToCraft && 
                    carried == null && 
                    actor.CanReachAndReserve(order.Workstation.Parent))
                    //feasibility.ArmedSlots.All(i => actor.CanReachAndReserve(i.Entity)))
                {
                    var plan = new Plan(PlanDefOf.Crafting, new TargetArgs(map, order.Workstation.Parent.OriginGlobal)) { Order = order, TargetB = new TargetArgs(order.Workstation.Parent) };

                    foreach (var allocation in feasibility.ArmedSlots)
                        plan.AddTarget(TargetIndex.A, allocation.Entity);
                    return plan;
                }

                // Carried item can be deposited
                if (carried != null)
                {
                    var carriedAlloc = feasibility.Allocations
                        .FirstOrDefault(a => a.Entity == carried);

                    if (carriedAlloc.Entity != null)
                    {
                        if (carried.StackSize >= carriedAlloc.Quantity)
                        {
                            // Fully satisfies → deposit now
                            return new Plan(PlanDefOf.GoPlace,
                                new TargetArgs(map, carriedAlloc.Slot))
                            {
                                AmountA = carriedAlloc.Quantity
                            };
                        }
                        else
                        {
                            // Partially satisfies → gather remainder first
                            var remainderAlloc = feasibility.Allocations
                                .First(a => a.Slot == carriedAlloc.Slot && a.Entity != carried);

                            return new Plan(PlanDefOf.GoHaul,
                                new TargetArgs(remainderAlloc.Entity))
                            {
                                AmountA = remainderAlloc.Quantity
                            };
                        }
                    }
                }

                // Otherwise, pick up next needed item
                if (carried != null)
                {
                    var correctAlloc = feasibility.Allocations.FirstOrDefault(a => carried.CanAbsorb(a.Entity));
                    if (correctAlloc.Entity != null)
                    {
                        return new Plan(PlanDefOf.GoHaul, new TargetArgs(correctAlloc.Entity))
                        {
                            AmountA = correctAlloc.Quantity
                        };
                    }
                }
                //foreach (var alloc in feasibility.Allocations)
                //{
                //    return new Plan(PlanDefOf.GoHaul, new TargetArgs(alloc.Entity))
                //    {
                //        AmountA = alloc.Quantity
                //    };
                //}

                // Consolidate allocations if they're from the same stack
                Entity targetStack = feasibility.Allocations.First().Entity;
                int totalQuantity = 0;
                foreach (var alloc in feasibility.Allocations)
                    if (alloc.Entity == targetStack)
                        totalQuantity += alloc.Quantity;
                return new Plan(PlanDefOf.GoHaul, new TargetArgs(targetStack))
                {
                    AmountA = totalQuantity
                };
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
                return new Plan(PlanDefOf.Repairing, repairableItem) { TargetB = new TargetArgs(map, workstation.Parent.OriginGlobal), TargetC = new TargetArgs(workstation.Parent) };

            if (actor.Hauled is Entity hauled && isRepairable(hauled))
                return new Plan(PlanDefOf.GoPlace, new TargetArgs(map, benchCell.Above)) { TargetB = new TargetArgs(workstation.Parent) };

            if (actor.Gear[GearTypeDefOf.Mainhand] is Entity repairableGear && isRepairable(repairableGear))
                return new Plan(PlanDefOf.Unequip, repairableGear);

            if (actor.Inventory.Contents.FirstOrDefault(isRepairable) is Entity repairableInvItem)
                return new Plan(PlanDefOf.RetrieveFromInventory, repairableInvItem);

            if (map.Stockpiles.GetItems(order.Workstation.Input).FirstOrDefault(isRepairable) is Entity repairableStockpileItem)
                return new Plan(PlanDefOf.GoHaul, repairableStockpileItem);

            return null;

            static bool isRepairable(Entity e) => e.Resources?[ResourceDefOf.Durability] is Resource durability && durability.Percentage < 1;
        }
    }
}
