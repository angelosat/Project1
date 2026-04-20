using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Crafting;
using Project1.Core.Towns.Duties;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

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

        if (map.Town.ServiceRequests.TryGetByVendor(actor, out ServiceRequest_Repair req))
        {
            var counter = req.Counter.Value;
            var item = world.Get(req.Item);
            var durability = item.Resources.GetPercentage(ResourceDefOf.Durability);

            if (durability < 1)
            {
                if (item.Cell == req.RepairBench.Value.Above)
                    return new Plan(PlanDefOf.Repairing, item);

                if (actor.Hauled == item)
                    return new Plan(PlanDefOf.GoPlace, map, req.RepairBench.Value.Above) { ServiceRequest = req.Id};

                if (actor.Hauled is null && item.IsSpawned)
                    return new Plan(PlanDefOf.GoHaul, item) { ServiceRequest = req.Id };

                return new Plan(TownServicesDefOf.PlanWaitItemSubmit, map, counter) { ServiceRequest = req.Id };
            }
            else
            {
                if (map.World.Get(req.Money) is Entity money)
                {
                    if (actor.Hauled == money)
                        // deposit money inside cash register
                        return new Plan(PlanDefOf.GoPlace, map, counter) { Continuation = PlanContinuationPolicy.Yield };

                    if (money.IsSpawned)
                    {
                        req.MarkPaidFor();
                        // if money is on counter, swap with item.
                        // if item not on counter, leave item on counter and go pickup money on next tick
                        
                        return new Plan(PlanDefOf.SwapCarried, money) { ServiceRequest = req.Id };
                    }
                }
                if (actor.Hauled == item)
                    return new Plan(TownServicesDefOf.PlanWaitMoney, map, counter) { ServiceRequest = req.Id };

                if (actor.Hauled is null && item.Cell != counter.Above)
                    return new Plan(PlanDefOf.GoHaul, item) { ServiceRequest = req.Id };

                return null;
            }
            throw new UnreachableException();
        }

        var bench = map.Town.Crafting.AllWorkstations
            .Where(e => e.WorkstationType.Capabilities.Contains(WorkstationCapabilityDefOf.Repairing))
            .FirstOrDefault(e => actor.CanReachAndReserve(e.Parent.OriginGlobal));

        if (bench is null)
            return null;

        foreach (var pending in map.Town.ServiceRequests.GetAllPendingRequests(TownServiceDefOf.Repairing))
        {
            if (pending is not ServiceRequest_Repair typed)
                continue;
            var customer = world.Get(pending.Customer);
            if (!actor.CanReachAndReserve(customer))
                continue;
            //manager.AssignVendor(typed, actor);
            map.Town.ServiceRequests.AssignVendor(req, actor);
            manager.AssignRepairBench(typed, bench.Parent.OriginGlobal);
            return null;
        }

        return null;
    }
}
