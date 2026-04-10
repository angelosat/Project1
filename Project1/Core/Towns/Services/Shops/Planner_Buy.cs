using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Services.Shops;

sealed class Planner_Buy : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (actor.IsTownMember)
            return null;

        var map = actor.Map;
        var manager = actor.ItemPreferences;
        var shops = map.Town.Shops;
        var servicepoints = map.Town.Shops.GetServicePoints();

        if (!servicepoints.Any())
            return null;
        var carried = actor.Hauled;

        if (shops.TryGetTransaction(actor, out var transaction))
        {
            var seller = map.World.Get<Actor>(transaction.Vendor);
            var item = map.World.Get(transaction.Item);

            if (carried is null)
            {
                //if (transaction.IsComplete)
                //{
                //    var (role, score) = manager.GetPotential(item);
                //    if (role is null)
                //        throw new Exception();
                //    manager.Commit(role, item, score);
                //    shops.FinishTransaction(actor);
                //    return null;
                //}
                if (transaction.IsProcessed)
                {
                    if (item.Cell == transaction.Counter.Above)
                    {
                        actor.AI.State.Log.Write($"I bought {item.Name}");
                        return new Plan(PlanDefOf.ClaimBoughtItem, item) { Continuation = PlanContinuationPolicy.Yield };
                    }
                }
                if (transaction.WaitingForPayment) // waiting for payment
                {
                    var price = transaction.Price;
                    var moneyInInventory = actor.Inventory.Contents.FirstOrDefault(i => i.Def == ItemDefOf.Coins);
                    if (moneyInInventory.StackSize < price)
                        throw new InvalidOperationException(); // normally the amount of coins should exist.
                                                               // maybe cancel the transaction gracefully if otherwise
                    return new Plan(PlanDefOf.RetrieveFromInventory, moneyInInventory) { AmountA = price };
                }
                //if (item.Cell == transaction.Counter.Above)
                //{
                //    if(transaction.IsProcessed)
                //        //if (transaction.IsPaid) // item paid for and ready to be claimed
                //    {
                //        actor.AI.State.Log.Write($"I bought {item.Name}");
                //        return new Plan(PlanDefOf.ClaimBoughtItem, item) { Continuation = PlanContinuationPolicy.Yield };
                //    }
                //    else // item on counter and waiting for clerk
                //        return new Plan(PlanDefOf.WaitForService);
                //}
                return new Plan(PlanDefOf.WaitForService);

            }
            if (carried is not null)
            {
                if (carried.Def == ItemDefOf.Coins && transaction.WaitingForPayment)
                    return new Plan(PlanDefOf.Pay, new InteractionTarget(map, transaction.Counter.Above));
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
                return new Plan(PlanDefOf.GoPlace, new InteractionTarget(map, transaction.Counter.Above));
            }
        }
        if (carried is not null)
            return null;
        var shoppingList = shops.GetShoppingListPopulated(actor);
        if (shoppingList.HasCompletedPurchaseThisVisit)
            return null;
        IntVec3 foundPoint = default;
        if (shoppingList.HasResults && !FindServicePoint(actor, servicepoints, out foundPoint))
            return null;
        //while (shoppingList.DequeueImpulse() is var impulse && impulse.item is Entity item)
        foreach(var (item, score, price) in shoppingList.GetResultsImpulse())
        {
            if (!IsValid(actor, item))
                continue;
            if (!map.Town.Shops.TryBeginTransaction(actor, item, price, foundPoint))
                continue;
            actor.AI.State.Log.Write($"I am impulsively buying {item.RefId}: {item.Name}!");
            return new Plan(PlanDefOf.GoHaul) { TargetA = item };
        }
        if (!shoppingList.HasFinished)
            return null;
        var potentialOrdered = shoppingList.GetResultsSorted();
        foreach (var (item, score, price) in potentialOrdered)
        {
            if (!IsValid(actor, item))
                continue;
            if (!map.Town.Shops.TryBeginTransaction(actor, item, price, foundPoint))
                continue;
            actor.AI.State.Log.Write($"I decided to buy {item.Name}");
            return new Plan(PlanDefOf.GoHaul) { TargetA = item };
        }
        return null;
    }

    private static bool IsValid(Actor actor, Entity item)
    {
        if (item.Map != actor.Map)
            return false;
        if (!item.IsForSale())
            return false;
        if (!actor.CanAfford(item))
            return false;
        if (item.IsInvolvedInExistingTransaction())
            return false;
        if (!actor.CanReachAndReserve(item))
            return false;
        return true;
    }

    private static bool FindServicePoint(Actor actor, IReadOnlySet<IntVec3> servicepoints, out IntVec3 foundPoint)
    {
        var validServicePointFound = false;
        foundPoint = default;
        foreach (var point in servicepoints)
        {
            if (actor.CanReachAndReserve(point))
            {
                validServicePointFound = true;
                foundPoint = point;
                break;
            }
        }
        return validServicePointFound;
    }
}
//sealed class PlannerBuy : Planner
//{
//    protected override Plan TryPlan(Actor actor)
//    {
//        if (actor.IsTownMember)
//            return null;

//        var map = actor.Map;
//        var manager = actor.ItemPreferences;
//        var shops = map.Town.ShopManager;
//        var servicepoints = map.Town.ShopManager.GetServicePoints();

//        if (!servicepoints.Any())
//            return null;

//        if (shops.TryGetTransaction(actor, out var transaction))
//        {
//            var seller = map.World.Get<Actor>(transaction.Seller);
//            var item = map.World.GetEntity(transaction.Item);

//            var carried = actor.Hauled;
//            if (carried is null)
//            {
//                if (transaction.IsComplete)
//                {
//                    var (role, score) = manager.GetPotential(item);
//                    if (role is null)
//                        throw new Exception();
//                    manager.Commit(role, item, score);
//                    shops.FinishTransaction(actor);
//                    return null;
//                    throw new Exception();
//                }
//                if (transaction.WaitingForPayment) // waiting for payment
//                {
//                    var price = transaction.Price;
//                    var moneyInInventory = actor.Inventory.Contents.FirstOrDefault(i => i.Def == ItemDefOf.Coins);
//                    if (moneyInInventory.StackSize < price)
//                        throw new InvalidOperationException(); // normally the amount of coins should exist.
//                                                               // maybe cancel the transaction gracefully if otherwise
//                    return new Plan(PlanDefOf.RetrieveFromInventory, moneyInInventory) { AmountA = price };
//                }
//                if (item.Cell == transaction.Counter.Above)
//                {
//                    if (transaction.IsPaid) // item paid for and ready to be claimed
//                        return new Plan(PlanDefOf.ClaimBoughtItem, item) { Continuation = PlanContinuationPolicy.Yield };
//                    else // item on counter and waiting for clerk
//                        return new Plan(PlanDefOf.WaitForService);
//                }
//            }
//            if (carried is not null)
//            {
//                if (carried.Def == ItemDefOf.Coins && transaction.WaitingForPayment)
//                    return new Plan(PlanDefOf.Pay, new TargetArgs(map, transaction.Counter.Above));
//                if (carried.RefId != transaction.Item)
//                    throw new InvalidOperationException();
//                transaction.Tick();
//                if (transaction.TimedOut)
//                {
//                    transaction.Cancel();
//                    return null;
//                }
//                if (!actor.CanReachAndReserve(transaction.Counter))
//                {
//                    transaction.Cancel();
//                    return null;
//                }
//                return new Plan(PlanDefOf.GoPlace, new TargetArgs(map, transaction.Counter.Above));
//            }
//        }

//        var shoppingList = shops.GetShoppingListPopulated(actor);
//        while(shoppingList.DequeueImpulse() is var impulse && impulse.item is Entity item)
//        {
//            if (item.Map != actor.Map)
//                continue;
//            if (!item.IsForSale())
//                continue;
//            if (!actor.CanAfford(item))
//                continue;
//            if (item.IsInvolvedInExistingTransaction())
//                continue;
//            if (!actor.CanReachAndReserve(item))
//                continue;
//            if (!FindServicePoint(actor, servicepoints, out var foundPoint))
//                return null;
//            if (!map.Town.ShopManager.TryBeginTransaction(actor, item, impulse.price, foundPoint))
//                return null;
//            return new Plan(PlanDefOf.GoHaul) { TargetA = item };
//        }
//        if (!shoppingList.HasFinished)
//            return null;
//        var potentialOrdered = shoppingList.GetResultsSorted();
//        foreach (var (item, score, price) in potentialOrdered)
//        {
//            if (item.Map != actor.Map)
//                continue;

//            if (!item.IsForSale())
//            {
//                manager.DiscardPotential(item);
//                return null;
//            }

//            if (!actor.CanAfford(item))
//            {
//                manager.DiscardPotential(item);
//                return null;
//            }

//            if (item.IsInvolvedInExistingTransaction())
//                continue;

//            if (!actor.CanReachAndReserve(item))
//                continue;

//            if (!FindServicePoint(actor, servicepoints, out var foundPoint))
//                return null;
//            if (!map.Town.ShopManager.TryBeginTransaction(actor, item, price, foundPoint))
//                return null;
//            return new Plan(PlanDefOf.GoHaul) { TargetA = item };
//        }
//        return null;
//    }

//    private static bool FindServicePoint(Actor actor, IReadOnlySet<IntVec3> servicepoints, out IntVec3 foundPoint)
//    {
//        var validServicePointFound = false;
//        foundPoint = default;
//        foreach (var point in servicepoints)
//        {
//            if (actor.CanReachAndReserve(point))
//            {
//                validServicePointFound = true;
//                foundPoint = point;
//                break;
//            }
//        }
//        return validServicePointFound;
//    }
//}
