using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.Entities;
using Project1.Core.Interactions;
using System;
using System.Collections.Generic;

namespace Project1.Core.Towns.Shops
{
    class TaskBehaviorAcceptSellHandToHand : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            // TODO wait for price negotiation
            // TODO send reply / complete transaction
            var actor = this.Actor;
            var state = this.Actor.AI.State;
            yield return new BehaviorStopMoving();
            yield return new BehaviorWait(() => state.TradingPartner == null);
            // if carrying coins, store in inventory. otherwise drop or haul to stockpile
            throw new NotImplementedException();

            //yield return new BehaviorResolveInteraction(TargetIndex.A, () => //this.Actor.Carried.Def == ItemDefOf.Coins ? new InteractionStoreHauled() : new InteractionThrow());
            //{
            //    if (this.Actor.Hauled.Def == ItemDefOf.Coins)
            //        return new InteractionStoreHauled();
            //    else
            //        return new InteractionThrow();
            //});
        }
    }
}
