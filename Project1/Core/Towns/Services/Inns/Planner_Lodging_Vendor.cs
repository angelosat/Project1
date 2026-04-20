using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Towns.Services.Inns;

internal sealed class Planner_Lodging_Vendor : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (!actor.IsTownMember)
            return null;

        var map = actor.Map;
        var manager = map.Town.Inns;
        if (manager.GetTransactionByClerk(actor) is ServiceRequest_Inn req)
        {
            if (req.IsPaidFor && actor.Hauled is null)
                    return new Plan(InnsDefOf.PlanRegisterGuest, new InteractionTarget(actor.Map, req.Counter.Value)) { ServiceRequest = req.Id };

            if (req.IsMoneyAllocated)
            {
                var money = map.World.Get<Entity>(req.Money);
                
                if (actor.Hauled == money)
                {
                    if(!manager.AvailableBeds.Any(b => actor.CanReach(b)))
                        throw new Exception();
                    var bed = manager.AvailableBeds.First(b => actor.CanReach(b));
                    return new Plan(PlanDefOf.GoPlace, new InteractionTarget(map, req.Counter.Value)) { ServiceRequest = req.Id };
                }
                if (money.Cell == req.Counter.Value.Above)
                {
                    req.MarkPaidFor();
                    return new Plan(PlanDefOf.GoHaul, money) { ServiceRequest = req.Id };
                }
            }

            throw new UnreachableException();
        }
        //var busyServicePoints = manager.GetServicePointsWithQueueUnserved();
        //foreach (var point in busyServicePoints)
        //{
        //    if (!manager.TryFindBedFrom(point, out var foundBed))
        //        continue;
        //    manager.AssignClerk(point, actor);
        //    return new Plan(InnsDefOf.PlanWaitForPayForBed, new InteractionTarget(actor.Map, point));
        //}
        if (!manager.AvailableBeds.Any(b => actor.CanReach(b)))
            return null;
        foreach (var pending in map.Town.ServiceRequests.GetAllPendingRequests(TownServiceDefOf.Lodging))
        {
            if (pending is not ServiceRequest_Inn typed)
                continue;
            //if (!manager.TryFindBedFrom(typed.Counter.Value, out var foundBed))
            //    continue;
            manager.AssignClerk(typed.Counter.Value, actor);
            //return new Plan(InnsDefOf.PlanWaitForPayForBed, actor.Map, typed.Counter.Value);
            return new Plan(TownServicesDefOf.PlanWaitMoney, actor.Map, typed.Counter.Value) { ServiceRequest = pending.Id };
        }
        return null;
    }
}
