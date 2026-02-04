using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Project1.Core.Interactions;
using Start_a_Town_;
using Project1.Core.AI.Behaviors.Pathing;

namespace Project1.Core.Towns.Shops
{
    class TaskBehaviorAcceptBuyHandToHand : BehaviorExecutePlan
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
                    this.Plan.TargetA = actor.GetMoney();
                    this.Plan.AmountA = tradingpartner.Hauled.GetValueTotal();
                    return true;
                }
                return false;
            });
            yield return new BehaviorResolveInteraction(TargetIndex.A, () => null);// new InteractionHaul(this.Plan.AmountA));
            yield return new BehaviorWait(() => state.TradingPartner == null);
            yield return new BehaviorResolveInteraction(TargetIndex.A, () => new InteractionStoreHauled());
        }
    }
}
