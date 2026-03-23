using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Framework;
using System;
using System.Linq;

namespace Project1.Core.Towns.Shops;

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
            var seller = map.World.Get<Actor>(transaction.Seller);
            var item = map.World.GetEntity(transaction.Item);

            var carried = actor.Hauled;
            //if (carried is null && map.GetEntitiesAt(transaction.Counter.Above).Any(e => e.RefId == transaction.Item))
            if (carried is null)// && map.World.GetEntity(transaction.Item).Cell == transaction.Counter.Above)
            {
                //var seller = map.World.GetEntity<Actor>(transaction.Seller);
                //if (seller.Hauled?.RefId == transaction.Item && transaction.Money == EntityRefId.Null) // waiting for payment
                if (transaction.IsComplete)
                {
                    var (role, score) = manager.GetPotential(item);
                    if (role is null)
                        throw new Exception();
                    manager.Commit(role, item, score);
                    shops.FinishTransaction(actor);
                    return null;
                    throw new Exception();
                }
                if (transaction.WaitingForPayment) // waiting for payment
                {
                    var moneyInInventory = actor.Inventory.Contents.FirstOrDefault(i => i.Def == ItemDefOf.Coins);
                    return new Plan(PlanDefOf.RetrieveFromInventory, moneyInInventory);
                }
                if (item.Cell == transaction.Counter.Above)
                {
                    if (transaction.IsPaid) // item paid for and ready to be claimed
                    {
                        //return new Plan(PlanDefOf.GoHaul, item) { Continuation = PlanContinuationPolicy.Yield };
                        return new Plan(PlanDefOf.ClaimBoughtItem, item) { Continuation = PlanContinuationPolicy.Yield };
                    }
                    else // item on counter and waiting for clerk
                        return new Plan(PlanDefOf.WaitForService);
                }
            }
            if (carried is not null)
            {
                if (carried.Def == ItemDefOf.Coins && transaction.WaitingForPayment)// seller.Hauled?.RefId == transaction.Item)
                {
                    //return new Plan(PlanDefOf.GoPlace, new TargetArgs(map, transaction.Counter.Above));
                    return new Plan(PlanDefOf.Pay, new TargetArgs(map, transaction.Counter.Above));
                }
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
            //return null;
        }
        

        var potentialAll = manager.GetPotential();
        foreach (var (role, item, score) in potentialAll)
        {
            if (!item.IsForSale())
            {
                manager.DiscardPotential(item);
                return null;
            }

            if (!actor.CanAfford(item))
            {
                manager.DiscardPotential(item);
                return null;
            }

            if (!actor.CanReachAndReserve(item))
                continue;

            bool validServicePointFound = false;
            IntVec3 foundPoint = default;
            foreach (var point in servicepoints)
            {
                if (actor.CanReachAndReserve(point))
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
