using System;
using System.Collections.Generic;
using Project1.Core.Interactions;
using Project1.Core.Entities;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.Entities.Actors;
using Project1.Core.AI.Behaviors.Helpers;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.Towns.Shops
{
    class TaskBehaviorBuy : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorResolvePath(TargetIndex.A);
            yield return BehaviorHaulHelper.StartCarrying(this, TargetIndex.A);
            // TODO start checking if the shop has a worker
            // if no worker or all workers busy, wait a bit and then cancel the behavior and drop town approval rating
            yield return new BehaviorResolvePath(TargetIndex.B);
            yield return new BehaviorResolveInteraction(TargetIndex.B, () => new InteractionGiveItem());
            // WARNING if coins in inventory have somehow been reduced below the item's cost since starting the behavior, cancel everything
            yield return new BehaviorCustom()
            {
                InitAction = () =>
                {
                    var item = this.Plan.TargetA.Object as Entity;
                    this.Plan.SetTarget(TargetIndex.A, this.Actor.Inventory.First(i => i.Def == ItemDefOf.Coins));
                    var totalvalue = item.GetValueTotal();
                    if (totalvalue <= 0)
                        throw new Exception();
                    this.Plan.SetAmount(TargetIndex.A, totalvalue);
                }
            };
            yield return new BehaviorResolveInteraction(TargetIndex.A, () => null);// new InteractionHaul(this.Plan.AmountA));
            yield return new BehaviorResolveInteraction(TargetIndex.B, () => new InteractionGiveItem(true));
            yield return new BehaviorCustom()
            {
                InitAction = () =>
                {
                    var target = this.Plan.TargetB.Object as Actor;
                    target.AI.State.TradingPartner = null;
                }
            };
            yield return new BehaviorResolveInteraction(() => new InteractionStoreHauled());
            // TODO behavior negotiate price
            // TODO behavior wait for reply
            // TODO complete transaction
            // TODO insert item to inventory
        }
        protected override bool ReserveExtra()
        {
            return
                this.ReserveAsManyAsPossible(this.Plan.TargetA, this.Plan.TargetA.Object.StackSize);
        }
    }
}
