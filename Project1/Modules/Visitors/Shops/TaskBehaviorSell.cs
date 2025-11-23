using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorSell : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorGetAtNewNew(TargetIndex.B);
            yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionHaul(this.Task.AmountA));
            var tradePartner = this.Task.TargetB.Object as Actor;
            var item = this.Task.TargetA.Object as Entity;
            var itemvalue = item.GetValueTotal();
            yield return new BehaviorWait(() =>
            {
                var carried = tradePartner.Hauled;
                if (carried == null)
                    return false;
                return carried.Def == ItemDefOf.Coins && carried.StackSize == itemvalue;
                // TODO cancel if not enouch coins?
            });
            yield return new BehaviorInteractionNew(TargetIndex.B, () => new InteractionGiveItem(true));
            yield return new BehaviorCustom()
            {
                InitAction = () =>
                {
                    tradePartner.GetState().TradingPartner = null;
                }
            };
            yield return new BehaviorInteractionNew(() => new InteractionStoreHauled());
        }
    }
}
