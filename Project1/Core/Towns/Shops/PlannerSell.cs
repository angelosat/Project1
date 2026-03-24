using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System;

namespace Project1.Core.Towns.Shops;

class PlannerSell : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (!actor.IsTownMember)
            return null;
    
        var map = actor.Map;
        var manager = map.Town.ShopManager;

        if(manager.TryGetTransactionBySeller(actor, out var existing))
        {
            var item = map.World.GetEntity(existing.Item);

            if (actor.Hauled == item)
            {
                if (!actor.CanReachAndReserve(existing.Counter))
                    throw new Exception();

                if (map.World.Get<Entity>(existing.Money) is Entity money && money.Cell == existing.Counter.Above && money.StackSize >= item.GetValueTotal())
                    return new Plan(PlanDefOf.RingUpFinish, money) { Continuation = PlanContinuationPolicy.Yield };

                return new Plan(PlanDefOf.WaitForPayment, new TargetArgs(map, existing.Counter.Above));
            }

            if (item.Cell != existing.Counter.Above)
                return null;

            if (!actor.CanReach(item))
                return null;

            return new Plan(PlanDefOf.RingUp, item);
        }

        foreach(var t in manager.PendingTransactions)
        {
            if (!actor.CanReachAndReserve(t.Counter))
                continue;

            var item = map.World.GetEntity(t.Item);
            if (item.Map != map)
                throw new Exception();

            manager.AssignSeller(t, actor);
        }
        return null;
    }
}
