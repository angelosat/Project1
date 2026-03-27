using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using Project1.Core.Resources;
using System;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Towns.Inns
{
    internal sealed class PlannerLodgingRegisterGuest : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.IsTownMember)
                return null;
            if (actor.IsHauling)
                return null;
            var map = actor.Map;
            var manager = map.Town.InnManager;
            //if (manager.GetTransactionByGuest(actor) is CheckInTransaction transaction)
            if (manager.GetTransactionByClerk(actor) is InnTransaction transaction)
            {
                if (transaction.IsFinished)
                {
                    var money = map.World.Get<Entity>(transaction.Money);
                    if (money.Cell != transaction.Desk.Above)
                        throw new Exception();
                    transaction.Dispose();
                    return new Plan(PlanDefOf.GoHaul, money);
                }
                if (transaction.IsPaid)
                //return null;
                {
                    if (!manager.TryFindBedFrom(transaction.Desk, out _))
                        throw new Exception();
                    return new Plan(InnsDefOf.PlanRegisterGuest, new TargetArgs(actor.Map, transaction.Desk));
                }
                throw new UnreachableException();
            }
            var busyServicePoints = manager.GetServicePointsWithQueue();
            foreach(var point in busyServicePoints)
            {
                if (!manager.TryFindBedFrom(point, out var foundBed))
                    continue;
                //return new Plan(InnsDefOf.PlanRegisterGuest, new TargetArgs(actor.Map, point));
                return new Plan(InnsDefOf.PlanWaitForPayForBed, new TargetArgs(actor.Map, point));
            }
            return null;
        }
    }
    internal sealed class PlannerLodgingCheckIn : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (actor.IsTownMember)
                return null;
            //if (actor.IsHauling)
            //    return null;
            var map = actor.Map;
            var manager = actor.Map.Town.InnManager;
            if (actor.HasCheckedIn)
                return null;
            var price = 100; // TODO: query manager for price
            if (manager.TryGetTransaction(actor, out var transaction))
            {
                
                if (transaction.IsPaid)
                    return new Plan(InnsDefOf.PlanCheckIn, new TargetArgs(map, transaction.Desk));
                if (transaction.IsAwaitingPayment)
                {
                    if (actor.Hauled is Entity carried)
                    {
                        if (carried.Def != ItemDefOf.Coins)
                            return null;
                        if (carried.StackSize != price)
                            return null;
                        return new Plan(InnsDefOf.PlanPayCheckIn, new TargetArgs(map, transaction.Desk.Above));
                    }
                    else
                    {
                        if (!actor.Inventory.TryGet(e => e.Def == ItemDefOf.Coins && e.StackSize >= price, out Entity money))
                            return null;
                        return new Plan(PlanDefOf.RetrieveFromInventory, money) { AmountA = price };
                    }
                }
                return null;
            }

            if (actor.Needs.GetPercentage(NeedDefOf.Energy) > .5f) // TODO: make it variable
                return null;
            if (actor.Resources.GetPercentage(ResourceDefOf.Patience) < .5f) // TODO: make it variable
                return null;
            var servicePoints = manager.GetServicePoints();
            if (!servicePoints.Any())
                return null;
            if (!actor.TryChoosePosition(servicePoints, out var desk))
                return null;
            //if (actor.Hauled is Entity carried)
            //{
            //    if (carried.Def != ItemDefOf.Coins)
            //        return null;
            //    if (carried.StackSize != price)
            //        return null;
            //    if (actor.HasCheckedIn)
            //        return new Plan(PlanDefOf.GoPlace, new TargetArgs(actor.Map, desk));

            //    return new Plan(InnsDefOf.PlanCheckIn, new TargetArgs(actor.Map, desk));
            //    //return new Plan(PlanDefOf.GoPlace, new TargetArgs(actor.Map, desk));
            //}
            //if (actor.Inventory.TryGet(e => e.Def == ItemDefOf.Coins && e.StackSize >= price, out Entity money))
            //{
            //    return new Plan(PlanDefOf.RetrieveFromInventory, money) { AmountA = price };
            //}
            // TODO: prefer smaller queues
            //if (!actor.TryChoosePosition(servicePoints, out var found))
            //    return null;
            if (!actor.Inventory.TryGet(e => e.Def == ItemDefOf.Coins && e.StackSize >= price, out _))
                return null;
            return new Plan(InnsDefOf.PlanCheckIn, new TargetArgs(actor.Map, desk));
        }

        //protected override Plan TryPlan(Actor actor)
        //{
        //    if (actor.IsTownMember)
        //        return null;
        //    //if (actor.IsHauling)
        //    //    return null;
        //    var manager = actor.Map.Town.InnManager;
        //    if (actor.HasCheckedIn)
        //        return null;
        //    var transaction = manager.GetTransaction(actor);

        //    if (actor.Needs.GetPercentage(NeedDefOf.Energy) > .5f) // TODO: make it variable
        //        return null;
        //    if (actor.Resources.GetPercentage(ResourceDefOf.Patience) < .5f) // TODO: make it variable
        //        return null;
        //    var servicePoints = manager.GetServicePoints();
        //    if (!servicePoints.Any())
        //        return null;
        //    if (!actor.TryChoosePosition(servicePoints, out var desk))
        //        return null;
        //    var price = 100; // TODO: query manager for price
        //    if(actor.Hauled is Entity carried)
        //    {
        //        if (carried.Def != ItemDefOf.Coins)
        //            return null;
        //        if (carried.StackSize != price)
        //            return null;
        //        if(actor.HasCheckedIn)
        //            return new Plan(PlanDefOf.GoPlace, new TargetArgs(actor.Map, desk));

        //        return new Plan(InnsDefOf.PlanCheckIn, new TargetArgs(actor.Map, desk));
        //        //return new Plan(PlanDefOf.GoPlace, new TargetArgs(actor.Map, desk));
        //    }
        //    if (actor.Inventory.TryGet(e => e.Def == ItemDefOf.Coins && e.StackSize >= price, out Entity money))
        //    {
        //        return new Plan(PlanDefOf.RetrieveFromInventory, money) { AmountA = price };
        //    }
        //    return null;
        //        // TODO: prefer smaller queues
        //    //if (!actor.TryChoosePosition(servicePoints, out var found))
        //    //    return null;

        //    //return new Plan(InnsDefOf.PlanCheckIn, new TargetArgs(actor.Map, desk));
        //}
    }
}
