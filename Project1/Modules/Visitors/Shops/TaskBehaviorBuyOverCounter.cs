using System;
using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorBuyOverCounter : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Task;
            
            this.FailOnRanOutOfPatienceWaiting(() => actor.GetVisitorProperties().BlacklistShop(task.ShopID));
            var item = task.TargetA.Object as Entity;

            this.AddFinishAction(() => actor.Town.ShopManager.GetShop<Shop>(task.ShopID).RemoveCustomer(actor));
            var counter = task.TargetB.Global;
            yield return new BehaviorGetAtNewNew(TargetIndex.A);
            yield return BehaviorHaulHelper.StartCarrying(this, TargetIndex.A);
            yield return BehaviorReserve.Reserve(this, TargetIndex.A); // reserve item so noone picks it up after placing it on the shop counter
            // TODO start checking if the shop has a worker
            // if no worker or all workers busy, wait a bit and then cancel the behavior and drop town approval rating
            yield return new BehaviorGetAtNewNew(TargetIndex.B);
            
           
            yield return new BehaviorInteractionNew(() => (actor.Map, counter.Above()), () => new UseHauledOnTarget());
            //        // TODO shop worker reserves dropped item immediately

            yield return new BehaviorWait(() => {
                // wait until a shop worker has picked the item up and is in their carry slot
                var worker = item.Parent; // HACK this will just wait for the first person to pick up the item. check if it's a worker of the current shop
                if (worker == null)
                    return false;
                if (worker.Hauled != item)
                    return false; // TODO maybe throw exception?
                return true;
                });
            // WARNING if coins in inventory have somehow been reduced below the item's cost since starting the behavior, cancel everything
            yield return new BehaviorCustom()
            {
                InitAction = () =>
                {
                    var item = this.Task.TargetA.Object as Entity;
                    this.Task.SetTarget(TargetIndex.C, this.Actor.Inventory.First(i => i.Def == ItemDefOf.Coins));
                    var totalvalue = item.GetValueTotal();
                    if (totalvalue <= 0)
                        throw new Exception();
                    this.Task.SetAmount(TargetIndex.C, totalvalue);
                }
            };
            yield return new BehaviorInteractionNew(TargetIndex.C, () => new InteractionHaul(this.Task.AmountC));
            //yield return new BehaviorCustom() { InitAction = () => actor.Reserve(this.Task, actor.Hauled) };
            yield return new BehaviorCustom() { InitAction = () => this.Reserve(actor.Hauled) };
            yield return new BehaviorInteractionNew(() => (actor.Map, counter.Above()), () => new UseHauledOnTarget());
            // TODO wait for the item to be placed ontop of the counter, and then pick it up
            yield return new BehaviorWait(() => item.Parent == null && item.Global.ToCell() == counter.Above());
            yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionHaul());
            yield return new BehaviorInteractionNew(() => new InteractionStoreHauled());
           
            // TODO behavior negotiate price
            // TODO behavior wait for reply
            // TODO complete transaction
            // TODO insert item to inventory
        }

        protected override bool InitExtraReservations()
        {
            return
                this.ReserveAsManyAsPossible(this.Task.TargetA, this.Task.TargetA.Object.StackSize);
        }
    }
}
