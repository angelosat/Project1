using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System;

namespace Project1.Core.Towns.Services.Shops;

sealed class PlannerBrowse : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (actor.IsTownMember)
            throw new Exception();
        var map = actor.Map;
        var shops = map.Town.Shops;
        if (!shops.HasServicePoints)
            return null;
        if (actor.IsHauling)
            return null;
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
