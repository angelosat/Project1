using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Project1.Core.Interactions;
using Project1.Core.Entities;
using Start_a_Town_;
using Project1.Core.AI.Behaviors.Pathing;

namespace Project1.Core.Towns.Shops
{
    class TaskBehaviorAcceptSellHandToHand : BehaviorExecutePlan
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
            yield return new BehaviorResolveInteraction(TargetIndex.A, () => //this.Actor.Carried.Def == ItemDefOf.Coins ? new InteractionStoreHauled() : new InteractionThrow());
            {
                if (this.Actor.Hauled.Def == ItemDefOf.Coins)
                    return new InteractionStoreHauled();
                else
                    return new InteractionThrow();
            });
        }
    }
}
