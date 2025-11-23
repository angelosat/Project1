using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorAcceptBuyHandToHand : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var state = this.Actor.GetState();
            var tradingpartner = state.TradingPartner;
          
            yield return new BehaviorStopMoving();
            yield return new BehaviorWait(() =>
            {
                if (tradingpartner.Hauled != null)
                {
                    this.Task.TargetA = actor.GetMoney();
                    this.Task.AmountA = tradingpartner.Hauled.GetValueTotal();
                    return true;
                }
                return false;
            });
            yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionHaul(this.Task.AmountA));
            yield return new BehaviorWait(() => state.TradingPartner == null);
            yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionStoreHauled());
        }
    }
}
