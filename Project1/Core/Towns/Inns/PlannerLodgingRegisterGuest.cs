using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System;
using System.Diagnostics;

namespace Project1.Core.Towns.Inns
{
    internal sealed class PlannerLodgingRegisterGuest : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.IsTownMember)
                return null;
            //if (actor.IsHauling)
            //    return null;
            var map = actor.Map;
            var manager = map.Town.InnManager;
            //if (manager.GetTransactionByGuest(actor) is CheckInTransaction transaction)
            if (manager.GetTransactionByClerk(actor) is ServiceRequest_Inn transaction)
            {
                //if (transaction.IsFinished)
                //{
                //    var money = map.World.Get<Entity>(transaction.Money);
                //    if (money.Cell != transaction.Desk.Above)
                //        throw new Exception();
                //    transaction.Dispose();
                //    return new Plan(PlanDefOf.GoHaul, money);
                //}
                if (transaction.IsProcessed && actor.Hauled is null)
                {
                    return new Plan(InnsDefOf.PlanRegisterGuest, new InteractionTarget(actor.Map, transaction.Desk));
                }
                if (transaction.IsPaid)
                //return null;
                {
                    var money = map.World.Get<Entity>(transaction.Money);
                    
                    //if()
                    if (actor.Hauled == money)
                    {
                        if (!manager.TryFindBedFrom(transaction.Desk, out _))
                            throw new Exception();
                        //transaction.MarkProcessed();
                        manager.MarkProcessed(transaction.Customer);
                        return new Plan(PlanDefOf.GoPlace, new InteractionTarget(map, transaction.Desk));
                        //return new Plan(InnsDefOf.PlanRegisterGuest, new TargetArgs(actor.Map, transaction.Desk));
                        //return new Plan(PlanDefOf.StoreInInventory);
                    }
                    if (money.Cell == transaction.Desk.Above)
                        return new Plan(PlanDefOf.GoHaul, money);

                    //if (!manager.TryFindBedFrom(transaction.Desk, out _))
                    //    throw new Exception();
                    //return new Plan(InnsDefOf.PlanRegisterGuest, new TargetArgs(actor.Map, transaction.Desk));
                }
                throw new UnreachableException();
            }
            var busyServicePoints = manager.GetServicePointsWithQueue();
            foreach(var point in busyServicePoints)
            {
                if (!manager.TryFindBedFrom(point, out var foundBed))
                    continue;
                //return new Plan(InnsDefOf.PlanRegisterGuest, new TargetArgs(actor.Map, point));
                return new Plan(InnsDefOf.PlanWaitForPayForBed, new InteractionTarget(actor.Map, point));
            }
            return null;
        }
    }
}
