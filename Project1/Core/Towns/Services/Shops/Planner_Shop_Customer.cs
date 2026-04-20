using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Services.Repairing;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Towns.Services.Shops;

sealed class Planner_Shop_Customer : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (actor.IsTownMember)
            return null;

        var map = actor.Map;
        var manager = actor.ItemPreferences;
        var shops = map.Town.Shops;
        //var servicepoints = map.Town.Shops.GetServicePoints();
        var servicepoints = map.Town.ServiceRequests.GetCounters(TownServiceDefOf.Buying);
        if (!servicepoints.Any())
            return null;
        var carried = actor.Hauled;

        if (shops.TryGetTransaction(actor, out var req))
        {
            var seller = map.World.Get<Actor>(req.Vendor);
            var item = map.World.Get(req.Item);
            var counter = req.Counter.Value;
            if (carried is null)
            {
                if (req.IsPaidFor)
                {
                    if (item.Cell == counter.Above)
                    {
                        actor.AI.State.Log.Write($"I bought {item.Name}");
                        return new Plan(PlanDefOf.ClaimBoughtItem, item) { Continuation = PlanContinuationPolicy.Yield };
                    }
                }
                if (req.IsVendorWaitingPayment && !req.IsMoneyAllocated) // waiting for payment
                {
                    var price = req.Price;
                    var moneyInInventory = actor.Inventory.Contents.FirstOrDefault(i => i.Def == ItemDefOf.Coins);
                    if (moneyInInventory.StackSize < price)
                        throw new InvalidOperationException(); // normally the amount of coins should exist.
                                                               // maybe cancel the transaction gracefully if otherwise
                    return new Plan(PlanDefOf.RetrieveFromInventory, moneyInInventory) { ServiceRequest = req.Id, AmountA = price };
                }
                return new Plan(PlanDefOf.WaitForService) { ServiceRequest = req.Id };

            }
            if (carried is not null)
            {
                if (!req.IsMoneyAllocated)
                {
                    if (carried.Def == ItemDefOf.Coins && req.IsVendorWaitingPayment)
                    {
                        carried.SetOwnerNew(null);
                        req.AllocateMoney(carried);
                    }
                    //else
                    //    return null;
                }

                if(carried == item)
                {
                    if(req.IsVendorWaitingItemSubmit)
                        return new Plan(PlanDefOf.GoPlace, map, counter.Above);
                    if (req.IsPaidFor)
                        return new Plan(PlanDefOf.StoreInInventory);
                    return new Plan(TownServicesDefOf.PlanQueue, map, counter.Above) { ServiceRequest = req.Id };
                }
                else if(carried.RefId == req.Money)
                    return new Plan(PlanDefOf.GoPlace, new InteractionTarget(map, counter.Above)) { ServiceRequest = req.Id };
                throw new UnreachableException();
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
        foreach (var (item, score, price) in shoppingList.GetResultsImpulse())
        {
            if (!IsValid(actor, item))
                continue;
            if (!map.Town.Shops.TryBegin(actor, item, price, foundPoint, out var reqnew))
                continue;
            actor.AI.State.Log.Write($"I am impulsively buying {item.RefId}: {item.Name}!");
            return new Plan(PlanDefOf.GoHaul) { ServiceRequest = reqnew.Id, TargetA = item };
        }
        if (!shoppingList.HasFinished)
            return null;
        var potentialOrdered = shoppingList.GetResultsSorted();
        foreach (var (item, score, price) in potentialOrdered)
        {
            if (!IsValid(actor, item))
                continue;
            if (!map.Town.Shops.TryBegin(actor, item, price, foundPoint, out var reqnew))
                continue;
            actor.AI.State.Log.Write($"I decided to buy {item.Name}");
            return new Plan(PlanDefOf.GoHaul) { ServiceRequest = reqnew.Id, TargetA = item };
        }
        return null;
    }

    private static bool IsValid(Actor actor, Entity item)
    {
        if (item.Map != actor.Map)
            return false;
        if (!item.IsForSale)
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
//sealed class Planner_Shop_Customer : Planner
//{
//    protected override Plan TryPlan(Actor actor)
//    {
//        if (actor.IsTownMember)
//            return null;

//        var map = actor.Map;
//        var manager = actor.ItemPreferences;
//        var shops = map.Town.Shops;
//        var servicepoints = map.Town.Shops.GetServicePoints();

//        if (!servicepoints.Any())
//            return null;
//        var carried = actor.Hauled;

//        if (shops.TryGetTransaction(actor, out var transaction))
//        {
//            var seller = map.World.Get<Actor>(transaction.Vendor);
//            var item = map.World.Get(transaction.Item);
//            var counter = transaction.Counter.Value;
//            if (carried is null)
//            {
//                if (transaction.IsPaidFor)
//                {
//                    if (item.Cell == counter.Above)
//                    {
//                        actor.AI.State.Log.Write($"I bought {item.Name}");
//                        return new Plan(PlanDefOf.ClaimBoughtItem, item) { Continuation = PlanContinuationPolicy.Yield };
//                    }
//                }
//                if (transaction.IsVendorWaitingPayment && !transaction.IsMoneyAllocated) // waiting for payment
//                {
//                    var price = transaction.Price;
//                    var moneyInInventory = actor.Inventory.Contents.FirstOrDefault(i => i.Def == ItemDefOf.Coins);
//                    if (moneyInInventory.StackSize < price)
//                        throw new InvalidOperationException(); // normally the amount of coins should exist.
//                                                               // maybe cancel the transaction gracefully if otherwise
//                    return new Plan(PlanDefOf.RetrieveFromInventory, moneyInInventory) { AmountA = price };
//                }
//                return new Plan(PlanDefOf.WaitForService);

//            }
//            if (carried is not null)
//            {
//                if (!transaction.IsMoneyAllocated)
//                {
//                    if (carried.Def == ItemDefOf.Coins && transaction.IsVendorWaitingPayment)
//                        transaction.AllocateMoney(carried);
//                    //else
//                    //    return null;
//                }
//                return new Plan(PlanDefOf.GoPlace, new InteractionTarget(map, counter.Above));
//            }
//        }
//        if (carried is not null)
//            return null;
//        var shoppingList = shops.GetShoppingListPopulated(actor);
//        if (shoppingList.HasCompletedPurchaseThisVisit)
//            return null;
//        IntVec3 foundPoint = default;
//        if (shoppingList.HasResults && !FindServicePoint(actor, servicepoints, out foundPoint))
//            return null;
//        //while (shoppingList.DequeueImpulse() is var impulse && impulse.item is Entity item)
//        foreach(var (item, score, price) in shoppingList.GetResultsImpulse())
//        {
//            if (!IsValid(actor, item))
//                continue;
//            if (!map.Town.Shops.TryBeginTransaction(actor, item, price, foundPoint))
//                continue;
//            actor.AI.State.Log.Write($"I am impulsively buying {item.RefId}: {item.Name}!");
//            return new Plan(PlanDefOf.GoHaul) { TargetA = item };
//        }
//        if (!shoppingList.HasFinished)
//            return null;
//        var potentialOrdered = shoppingList.GetResultsSorted();
//        foreach (var (item, score, price) in potentialOrdered)
//        {
//            if (!IsValid(actor, item))
//                continue;
//            if (!map.Town.Shops.TryBeginTransaction(actor, item, price, foundPoint))
//                continue;
//            actor.AI.State.Log.Write($"I decided to buy {item.Name}");
//            return new Plan(PlanDefOf.GoHaul) { TargetA = item };
//        }
//        return null;
//    }

//    private static bool IsValid(Actor actor, Entity item)
//    {
//        if (item.Map != actor.Map)
//            return false;
//        if (!item.IsForSale())
//            return false;
//        if (!actor.CanAfford(item))
//            return false;
//        if (item.IsInvolvedInExistingTransaction())
//            return false;
//        if (!actor.CanReachAndReserve(item))
//            return false;
//        return true;
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

