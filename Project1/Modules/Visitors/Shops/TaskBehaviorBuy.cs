using System;
using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorBuy : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorGetAtNewNew(TargetIndex.A);
            yield return BehaviorHaulHelper.StartCarrying(this, TargetIndex.A);
            // TODO start checking if the shop has a worker
            // if no worker or all workers busy, wait a bit and then cancel the behavior and drop town approval rating
            yield return new BehaviorGetAtNewNew(TargetIndex.B);
            yield return new BehaviorInteractionNew(TargetIndex.B, () => new InteractionGiveItem());
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
            yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionHaul(this.Plan.AmountA));
            yield return new BehaviorInteractionNew(TargetIndex.B, () => new InteractionGiveItem(true));
            yield return new BehaviorCustom()
            {
                InitAction = () =>
                {
                    var target = this.Plan.TargetB.Object as Actor;
                    target.GetState().TradingPartner = null;
                }
            };
            yield return new BehaviorInteractionNew(() => new InteractionStoreHauled());
            // TODO behavior negotiate price
            // TODO behavior wait for reply
            // TODO complete transaction
            // TODO insert item to inventory
        }
        protected override bool InitExtraReservations()
        {
            return
                this.ReserveAsManyAsPossible(this.Plan.TargetA, this.Plan.TargetA.Object.StackSize);
        }
    }
}
