using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;
using Project1.Framework;
using System;
using System.Linq;

namespace Project1.Core.Towns.Shops;

class PlannerSell : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (!actor.IsTownMember)
            return null;
        if (actor.IsHauling)
            return null;
        var map = actor.Map;
        var manager = map.Town.ShopManager;

        if(manager.TryGetTransactionBySeller(actor, out var existing))
        {
            var item = map.World.GetEntity(existing.Item);

            if (item.Cell != existing.Counter.Above)
                return null;

            if (!actor.CanReach(item))
                return null;

            return new Plan(PlanDefOf.GoHaul, item);
        }

        foreach(var t in manager.PendingTransactions)
        {
            if (!actor.CanReachAndReserve(t.Counter))
                continue;

            var item = map.World.GetEntity(t.Item);
            if (item.Map != map)
                throw new Exception();

            // wait until the item is on the counter?
            if (item.Cell != t.Counter.Above)
                continue;
            
            manager.AssignSeller(t, actor);

            
        }
        return null;
    }
}
class PlannerBuy : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (actor.IsTownMember)
            return null; 

        var map = actor.Map;
        var manager = actor.ItemPreferences;
        var shops = map.Town.ShopManager;
        var servicepoints = map.Town.ShopManager.GetServicePoints();

        if (!servicepoints.Any())
            return null;

        if (shops.TryGetTransaction(actor, out var transaction))
        {
            var carried = actor.Hauled;
            if (carried is null && map.GetEntitiesAt(transaction.Counter.Above).Any(e => e.RefId == transaction.Item))
            {
                return new Plan(PlanDefOf.WaitForService);
            }
            if (carried is not null)
            {
                if (carried.RefId != transaction.Item)
                    throw new InvalidOperationException();
                transaction.Tick();
                if (transaction.TimedOut)
                {
                    transaction.Cancel();
                    return null;
                }
                if (!actor.CanReachAndReserve(transaction.Counter))
                {
                    transaction.Cancel();
                    return null;
                }
                return new Plan(PlanDefOf.GoPlace, new TargetArgs(map, transaction.Counter.Above));
            }
            return null;
        }

        var potentialAll = manager.GetPotential();
        foreach (var (role, item, score) in potentialAll)
        {
            if (!item.IsForSale())
            {
                manager.DiscardPotential(item);
                return null;
            }

            if(!actor.CanAfford(item))
            {
                manager.DiscardPotential(item);
                return null;
            }

            if (!actor.CanReachAndReserve(item))
                continue;

            bool validServicePointFound = false;
            IntVec3 foundPoint = default;
            foreach(var point in servicepoints)
            {
                if(actor.CanReachAndReserve(point))
                {
                    validServicePointFound = true;
                    foundPoint = point;
                    break;
                }
            }
            if (!validServicePointFound)
                return null;
            if (!map.Town.ShopManager.TryBeginTransaction(actor, item, foundPoint))
                return null;
            return new Plan(PlanDefOf.GoHaul) { TargetA = item };
        }
        return null;
    }
}
