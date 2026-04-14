using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System;
using System.Diagnostics;

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
                    return new Plan(InnsDefOf.PlanRegisterGuest, new InteractionTarget(actor.Map, req.Counter.Value));

            if (req.IsMoneyAllocated)
            {
                var money = map.World.Get<Entity>(req.Money);
                
                if (actor.Hauled == money)
                {
                    if (!manager.TryFindBedFrom(req.Counter.Value, out _))
                        throw new Exception();
                    return new Plan(PlanDefOf.GoPlace, new InteractionTarget(map, req.Counter.Value));
                }
                if (money.Cell == req.Counter.Value.Above)
                {
                    req.MarkPaidFor();

                    return new Plan(PlanDefOf.GoHaul, money);
                }
            }

            throw new UnreachableException();
        }
        var busyServicePoints = manager.GetServicePointsWithQueueUnserved();
        foreach (var point in busyServicePoints)
        {
            if (!manager.TryFindBedFrom(point, out var foundBed))
                continue;
            manager.AssignClerk(point, actor);
            return new Plan(InnsDefOf.PlanWaitForPayForBed, new InteractionTarget(actor.Map, point));
        }
        return null;
    }
}
