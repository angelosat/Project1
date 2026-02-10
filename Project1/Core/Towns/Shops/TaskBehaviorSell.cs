using System;
using System.Collections.Generic;
using Project1.Core.Interactions;
using Project1.Core.Entities;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.Entities.Actors;
using Project1.Core.AI.Behaviors;
using Project1.Core.Towns.Trading;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.Towns.Shops
{
    class TaskBehaviorSell : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolvePath(TargetIndex.B);
            throw new NotImplementedException();
            yield return new BehaviorResolveInteraction(TargetIndex.A, () => null);
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
                    tradePartner.AI.State.TradingPartner = null;
                }
            };
            yield return new BehaviorResolveInteraction(() => new InteractionStoreHauled());
        }
    }
}
