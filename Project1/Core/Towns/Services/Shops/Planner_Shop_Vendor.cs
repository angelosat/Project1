using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System;

namespace Project1.Core.Towns.Services.Shops;

class Planner_Shop_Vendor : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (!actor.IsTownMember)
            return null;

        var map = actor.Map;
        var manager = map.Town.Shops;

        if (manager.TryGetTransactionBySeller(actor, out var req))
        {
            var counter = req.Counter.Value;

            if (req.IsPaidFor && actor.Hauled?.RefId == req.Money)
                return new Plan(PlanDefOf.GoPlace, map, counter) { Continuation = PlanContinuationPolicy.Yield };

            var item = map.World.Get(req.Item);

            if (actor.Hauled == item)
            {
                if (!actor.CanReachAndReserve(counter))
                    throw new Exception();

                if (map.World.Get<Entity>(req.Money) is Entity money &&
                    money.Cell == counter.Above &&
                    money.StackSize >= req.Price)
                {
                    return new Plan(PlanDefOf.RingUpFinish, money) { ServiceRequest = req, AmountA = req.Price };//, Continuation = PlanContinuationPolicy.Yield };
                }
                return new Plan(TownServicesDefOf.PlanWaitMoney) { ServiceRequest = req };
            }
            if (actor.Hauled is not null)
                throw new InvalidOperationException("actor shouldn't be carrying something unrelated at this point");
            if (req.IsItemSubmitted(actor.World))
                return new Plan(PlanDefOf.GoHaul, item) { ServiceRequest = req };
            return new Plan(TownServicesDefOf.PlanWaitItemSubmit, map, counter) { ServiceRequest = req };

        }

        foreach (var t in manager.PendingTransactions)
        {
            var tcounter = t.Counter.Value;
            if (!actor.CanReachAndReserve(tcounter))
                continue;
            manager.AssignSeller(t, actor);
        }
        return null;
    }
}
