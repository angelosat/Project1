using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System;

namespace Project1.Core.Towns.Shops;

sealed class PlannerBrowse : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (actor.IsTownMember)
            throw new Exception();
        var map = actor.Map;
        var shops = map.Town.ShopManager;
        var list = shops.GetShoppingListPopulated(actor);
        if (actor.IsHauling)
            return null;
        //while(list.Peek() is Entity item)
        while(list.Dequeue() is Entity item)
        {
            if (item.Map != map)
                continue;
            if (!actor.CanReachAndReserve(item))
                continue;
            return new Plan(PlanDefOf.BrowseProduct, item);
        }
        return null;
    }
}
