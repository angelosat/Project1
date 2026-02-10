using System;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Towns.Shops
{
    class TaskGiverTrading : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var state = actor.AI.State;
            var tradepartner = state.TradingPartner;
            if (tradepartner == null)
                return null;
            var partnerbhav = tradepartner.CurrentTask.BehaviorType;
            if (partnerbhav == typeof(TaskBehaviorBuy))
                return new Plan(typeof(TaskBehaviorAcceptSellHandToHand));
            else if (partnerbhav == typeof(TaskBehaviorSell))
                return new Plan(typeof(TaskBehaviorAcceptBuyHandToHand));
            else
                throw new Exception();
        }
    }
}
