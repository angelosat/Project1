using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System;
using System.Diagnostics;

namespace Project1.Core.Towns.Services.Inns;

internal sealed class PlannerLodgingRegisterGuest : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (!actor.IsTownMember)
            return null;

        var map = actor.Map;
        var manager = map.Town.Inns;
        if (manager.GetTransactionByClerk(actor) is ServiceRequest_Inn transaction)
        {
            if (transaction.IsProcessed && actor.Hauled is null)
                return new Plan(InnsDefOf.PlanRegisterGuest, new InteractionTarget(actor.Map, transaction.Desk));

            if (transaction.IsPaid)
            {
                var money = map.World.Get<Entity>(transaction.Money);
                
                if (actor.Hauled == money)
                {
                    if (!manager.TryFindBedFrom(transaction.Desk, out _))
                        throw new Exception();
                    manager.MarkProcessed(transaction.Customer);
                    return new Plan(PlanDefOf.GoPlace, new InteractionTarget(map, transaction.Desk));
                }
                if (money.Cell == transaction.Desk.Above)
                    return new Plan(PlanDefOf.GoHaul, money);
            }

            throw new UnreachableException();
        }
        //var busyServicePoints = manager.GetServicePointsWithQueue();
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
