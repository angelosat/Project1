using System;

namespace Start_a_Town_
{
    class TaskGiverTrading : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var state = actor.GetState();
            var tradepartner = state.TradingPartner;
            if (tradepartner == null)
                return null;
            var partnerbhav = tradepartner.CurrentTask.BehaviorType;
            if (partnerbhav == typeof(TaskBehaviorBuy))
                return new AITask(typeof(TaskBehaviorAcceptSellHandToHand));
            else if (partnerbhav == typeof(TaskBehaviorSell))
                return new AITask(typeof(TaskBehaviorAcceptBuyHandToHand));
            else
                throw new Exception();
        }
    }
}
