using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorSell : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolvePath(TargetIndex.B);
            yield return new BehaviorResolveInteraction(TargetIndex.A, () => new InteractionHaul(this.Plan.AmountA));
            var tradePartner = this.Plan.TargetB.Object as Actor;
            var item = this.Plan.TargetA.Object as Entity;
            var itemvalue = item.GetValueTotal();
            yield return new BehaviorWait(() =>
            {
                var carried = tradePartner.Hauled;
                if (carried == null)
                    return false;
                return carried.Def == ItemDefOf.Coins && carried.StackSize == itemvalue;
                // TODO cancel if not enouch coins?
            });
            yield return new BehaviorResolveInteraction(TargetIndex.B, () => new InteractionGiveItem(true));
            yield return new BehaviorCustom()
            {
                InitAction = () =>
                {
                    tradePartner.GetState().TradingPartner = null;
                }
            };
            yield return new BehaviorResolveInteraction(() => new InteractionStoreHauled());
        }
    }
}
