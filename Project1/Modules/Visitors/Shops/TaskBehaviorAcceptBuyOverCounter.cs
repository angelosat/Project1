using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorAcceptBuyOverCounter : BehaviorPerformTask
    {
        const TargetIndex Counter = TargetIndex.A;
        const TargetIndex Money = TargetIndex.B;
        const TargetIndex Item = TargetIndex.C;
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var net = actor.Net;
            var state = this.Actor.GetState();
            var task = this.Task;
            var shopID = task.ShopID;
            var transaction = task.Transaction;
            var customer = transaction.Customer;
            var item = net.GetNetworkEntity(transaction.Item);
            var cost = transaction.Cost;
            var shop = actor.Map.Town.GetShop(shopID) as Shop;
            var counterSurface = shop.Counter.Value.Above;

            this.FailOn(() => !shop.IsValid());

            // TODO failon not enough money?
            // TODO failon customer failed behavior
            // TODO failon transaction cancelled
            // TODO failon shop invalid (no counter, deleted) (don't fail on no worker, we need the customer to be dissatisfied if not served
            // TODO decrease customer's approval if shop becomes invalidated (intentionally or not) while the behavior is running

            yield return BehaviorHelper.SetTarget(Item, item);
            yield return BehaviorHelper.SetTarget(Counter, shop.Counter.Value); 
            yield return BehaviorHelper.MoveTo(Counter);
            yield return new BehaviorWait(() => item.CellIfSpawned == counterSurface);
            yield return new BehaviorCustom()
            { 
                InitAction = ()=>
                {
                    var money = actor.GetMoney();
                    task.SetTarget(Money, money, cost);
                }
            };
            yield return BehaviorHelper.CarryFromInventoryAndReplaceTarget(Money); // this assigns the new split object to the same target index
            yield return BehaviorReserve.Reserve(Money); // this reserves just the source object and not the new object from splitting the source object (but CarryFromInventoryAndReplaceTarget replaces the target with the new object)

            yield return new BehaviorInteractionNew(Item, () => new InteractionSwapCarried());
            yield return new BehaviorCustom() { InitAction = () => shop.RemoveCustomer(customer) };
            yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionStoreHauled());
        }
    }
}
