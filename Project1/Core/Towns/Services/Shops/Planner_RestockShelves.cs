using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;
using System.Linq;

namespace Project1.Core.Towns.Services.Shops;

internal class Planner_RestockShelves : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        var map = actor.Map;
        var manager = map.Town.Shops;

        var carried = actor.Hauled;

        if (carried is not null)
        {
            if (!manager.IsForSale(carried))
                return null;
            foreach (var shelf in manager.GetShelvesForItem(carried))
            {
                if (!actor.CanReachAndReserve(shelf.Above))
                    continue;
                return new Plan(PlanDefOf.GoPlace, map, shelf.Above) { Continuation = PlanContinuationPolicy.Yield };
            }
            return null;
        }
        var items = manager.GetItemsNeedingRestock();
        foreach (var item in items)
        {
            if (!actor.CanReachAndReserve(item))
                continue;
            foreach (var shelf in manager.GetShelvesForItem(item))
            {
                if (!actor.CanReachAndReserve(shelf.Above))
                    continue;
                return new Plan(PlanDefOf.GoHaul, item);
            }
        }

        return null;
    }

    //protected override Plan TryPlan(Actor actor)
    //{
    //    var map = actor.Map;
    //    var manager = map.Town.Shops;

    //    var product = actor.Hauled;
    //    if (product is not null && !manager.IsForSale(product))
    //        return null;

    //    var emptyShelves = manager.EmptyShelves;
    //    foreach (var shelf in emptyShelves)
    //    {
    //        if (!actor.CanReachAndReserve(shelf.Above))
    //            continue;
    //        if (product is not null && manager.CanShelfAccept(shelf, product))
    //            return new Plan(PlanDefOf.GoPlace, map, shelf.Above) { Continuation = PlanContinuationPolicy.Yield };
    //        foreach (var item in manager.GetItemsMarkedForSale())
    //        {
    //            if (!actor.CanReachAndReserve(item))
    //                continue;
    //            return new Plan(PlanDefOf.GoHaul, item);
    //        }
    //    }
    //    return null;
    //}
}
