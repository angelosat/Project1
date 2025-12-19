using System;

namespace Start_a_Town_
{
    class TaskGiverTrading : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var state = actor.GetState();
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
