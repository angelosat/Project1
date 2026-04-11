using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System;

namespace Project1.Core.Towns.Services.Shops;

class Planner_Sell : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (!actor.IsTownMember)
            return null;
    
        var map = actor.Map;
        var manager = map.Town.Shops;

        if(manager.TryGetTransactionBySeller(actor, out var transaction))
        {
            var counter = transaction.Counter.Value;
            if (actor.Hauled?.RefId == transaction.Money)
            if(transaction.IsProcessed)
            {
                return new Plan(PlanDefOf.GoPlace, new InteractionTarget(map, counter)){ Continuation = PlanContinuationPolicy.Yield };
                
            }
            var item = map.World.Get(transaction.Item);

            if (actor.Hauled == item)
            {
                if (!actor.CanReachAndReserve(counter))
                    throw new Exception();

                if (map.World.Get<Entity>(transaction.Money) is Entity money &&
                    money.Cell == counter.Above &&
                    money.StackSize >= transaction.Price)
                    return new Plan(PlanDefOf.RingUpFinish, money) { AmountA = transaction.Price };//, Continuation = PlanContinuationPolicy.Yield };

                return new Plan(PlanDefOf.WaitForPayment, new InteractionTarget(map, counter.Above));
            }
            if (transaction.IsProcessed)
                return null;
            if (item.Cell != counter.Above)
                return null;

            if (!actor.CanReach(item))
                return null;

            return new Plan(PlanDefOf.RingUp, item);
        }

        foreach(var t in manager.PendingTransactions)
        {
            var tcounter = t.Counter.Value;
            if (!actor.CanReachAndReserve(tcounter))
                continue;

            var item = map.World.Get(t.Item);
            if (item.Map != map)
                return null; // only go ahead and assign seller if the item is on the counter
            if (item.Cell != tcounter.Above)
                return null;

            manager.AssignSeller(t, actor);
        }
        return null;
    }
}
