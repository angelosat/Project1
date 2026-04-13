using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System;
using System.Linq;

namespace Project1.Core.Towns.Services.Shops;

sealed class Planner_Shop_Browse : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (actor.IsTownMember)
            throw new Exception();
        var map = actor.Map;
        //if (!shops.HasServicePoints)
        //    return null;
        var servicepoints = map.Town.ServiceRequests.GetCounters(TownServiceDefOf.Buying);
        if (!servicepoints.Any())
            return null;
        if (actor.IsHauling)
            return null;
        var shops = map.Town.Shops;
        var list = shops.GetShoppingListPopulated(actor);
        if (list.HasCompletedPurchaseThisVisit)
            return null;
        while (list.Dequeue() is Entity item)
        {
            if (actor.AI.State.Knowledge.TryQuery(item, out _))
                continue;
            if (item.Map != map)
                continue;
            if (!actor.CanReachAndReserve(item))
                continue;
            return new Plan(PlanDefOf.BrowseProduct, item) { Continuation = PlanContinuationPolicy.Yield };
        }
        return null;
    }
}
