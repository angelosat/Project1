using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Personality;
using Project1.Core.Crafting;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Towns.Duties;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Towns.Services.Repairing;

internal sealed class Planner_Repairs_Vendor : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (!actor.HasDuty(DutyDefOf.Repairsmith))
            return null;
        var map = actor.Map;
        var world = map.World;
        var manager = actor.Map.Town.Repairs;

        if(map.Town.ServiceRequests.TryGetByVendor(actor, out ServiceRequest_Repair req))
        {
            var counter = req.Counter.Value;
            var item = world.Get(req.Item);
            var durability = item.Resources.GetPercentage(ResourceDefOf.Durability);

            if (durability < 1)
            {
                if (item.Cell == req.RepairBench.Value.Above)
                    return new Plan(PlanDefOf.Repairing, item);

                if (actor.Hauled == item)
                    return new Plan(PlanDefOf.GoPlace, map, req.RepairBench.Value.Above);

                if (actor.Hauled is null && item.Cell == counter.Above)
                    return new Plan(PlanDefOf.GoHaul, item);

                return new Plan(ServiceRepairsDefOf.PlanQueueServe, map, counter) { ServiceRequest = req };
            }
            else
            {
                var money = map.World.Get(req.Money);

                if(money is not null && actor.Hauled == money)
                    return new Plan(PlanDefOf.GoPlace, map, counter) { Continuation = PlanContinuationPolicy.Yield };

                if (money is not null && money.Cell == counter.Above)
                {
                    req.MarkVendorPaid();
                    return new Plan(PlanDefOf.SwapCarried, money);
                }
                if (actor.Hauled == item)
                    return new Plan(ServiceRepairsDefOf.PlanWaitMoney, map, counter) { ServiceRequest = req };


                if (actor.Hauled is null && item.Cell != counter.Above)
                    return new Plan(PlanDefOf.GoHaul, item);

                return null;
            }
            throw new UnreachableException();
        }

        var bench = map.Town.CraftingManager.AllWorkstations
            .Where(e => e.WorkstationType.Capabilities.Contains(WorkstationCapabilityDefOf.Repairing))
            .FirstOrDefault(e => actor.CanReachAndReserve(e.Parent.OriginGlobal));

        if (bench is null)
            return null;

        foreach (var pending in map.Town.ServiceRequests.GetAllPendingRequests(TownServiceDefOf.Repairing))
        {
            var typed = (ServiceRequest_Repair)pending;
            var customer = world.Get(pending.Customer);
            if (!actor.CanReachAndReserve(customer))
                continue;
            manager.AssignVendor(typed, actor);
            manager.AssignRepairBench(typed, bench.Parent.OriginGlobal);
            return null;
        }

        return null;
    }
}
internal sealed class Planner_Repairs_Customer : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        var map = actor.Map;
        var manager = actor.Map.Town.Repairs;

        if (manager.TryGetByCustomer(actor, out var existing))
        {
            var counter = existing.Counter.Value;

            var item = actor.World.Get(existing.Item);
            var durability = item.Resources.GetPercentage(ResourceDefOf.Durability);
            if (durability >= 1)
            {
                if (actor.Hauled == item)
                    return new Plan(PlanDefOf.StoreInInventory) { Continuation = PlanContinuationPolicy.Yield };
                if (actor.Inventory.Contains(item))
                {
                    existing.MarkSuccess();
                    return null;
                }
                if(existing.IsVendorPaid)
                {
                    if (actor.Hauled == item)
                        return new Plan(PlanDefOf.StoreInInventory);
                    if (item.Cell == counter.Above)
                        return new Plan(PlanDefOf.GoHaul, item);
                    return new Plan(ServiceRepairsDefOf.PlanWaitItem) { ServiceRequest = existing };
                }
                if (existing.IsVendorWaitingPayment)
                {
                    if (existing.Money != EntityRefId.Null)
                    {
                        var itemMoney = actor.World.Get(existing.Money);
                        if (itemMoney.Cell == counter.Above)
                            return new Plan(ServiceRepairsDefOf.PlanWaitItem) { ServiceRequest = existing };
                    }
                    if (actor.Hauled is Entity carriedMoney && carriedMoney.Def == ItemDefOf.Coins && carriedMoney.StackSize == existing.Price)
                    {
                        existing.Money = carriedMoney.RefId;
                        return new Plan(PlanDefOf.GoPlace, map, counter.Above);
                    }
                    var money = actor.Inventory.FirstToTake(e => e.Def == ItemDefOf.Coins, existing.Price) 
                        ?? throw new UnreachableException("money should never be null if we've reached this point");
                    return new Plan(PlanDefOf.RetrieveFromInventory, money) { AmountA = existing.Price };

                }
                throw new UnreachableException();
            }
            if (existing.IsVendorWaiting && actor.Hauled == item)
                return new Plan(PlanDefOf.GoPlace, map, counter.Above);

            if (existing.IsVendorWorking)
                return new Plan(ServiceRepairsDefOf.PlanQueueWait, map, counter.Above) { ServiceRequest = existing };

            if (actor.Hauled is Entity carried)
            {
                if (carried != item)
                    return null;

                return new Plan(ServiceRepairsDefOf.PlanQueue, map, counter.Above) { ServiceRequest = existing };
            }
            return new Plan(PlanDefOf.RetrieveFromInventory, item);
        }

        var inventory = actor.Inventory;
        var durThreshold = .5f + actor.Personality.GetPercentage(TraitDefOf.Deliberation) / 2f;
        var damaged = inventory.Score(e => e.Resources?.GetPercentageOrDefault(ResourceDefOf.Durability));

        var mostDamaged = damaged
            .Where(e => e.score < durThreshold)
            .OrderBy(i => i.score)
            .FirstOrDefault();

        if (mostDamaged.item is null)
            return null;

        var counters = map.Town.ServiceRequests.GetCounters(TownServiceDefOf.Repairing);
        foreach(var counter in counters)
        {
            if (!actor.CanReachAndReserve(counter))
                continue;
            manager.Begin(actor, mostDamaged.item, (int)(mostDamaged.score * 100), counter);
            return null;
        }

        return null;
    }
}
