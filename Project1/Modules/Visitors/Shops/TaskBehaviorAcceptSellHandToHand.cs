using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorAcceptSellHandToHand : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            // TODO wait for price negotiation
            // TODO send reply / complete transaction
            var actor = this.Actor;
            var state = this.Actor.GetState();
            yield return new BehaviorStopMoving();
            yield return new BehaviorWait(() => state.TradingPartner == null);
            // if carrying coins, store in inventory. otherwise drop or haul to stockpile
            yield return new BehaviorInteractionNew(TargetIndex.A, () => //this.Actor.Carried.Def == ItemDefOf.Coins ? new InteractionStoreHauled() : new InteractionThrow());
            {
                if (this.Actor.Hauled.Def == ItemDefOf.Coins)
                    return new InteractionStoreHauled();
                else
                    return new InteractionThrow();
            });
        }
    }
}
