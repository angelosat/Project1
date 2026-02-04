using System.Collections.Generic;
using Start_a_Town_.Framework.AI.NodeTypes;
using Start_a_Town_.AI.Behaviors;
using Project1.Core.Interactions;
using Project1.Core.Entities;
using Start_a_Town_;

namespace Project1.Core.Towns.Shops
{
    class TaskBehaviorSellOverCounter : BehaviorExecutePlan
    {
        const TargetIndex Item = TargetIndex.A;
        const TargetIndex Counter = TargetIndex.B;
        const TargetIndex Money = TargetIndex.C;

        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var state = this.Actor.GetState();
            var task = this.Plan;
            var shop = actor.Town.ShopManager.GetShop<Shop>(task.ShopID);
            var transaction = task.Transaction;
            var customer = transaction.Customer;
            var item = actor.World.GetEntity(transaction.Item);
            var cost = transaction.Cost;
            var counter = shop.Counter.Value;
            var counterSurface = counter.Above;

            this.FailOn(() => !shop.IsValid());
            this.FailOnRanOutOfPatienceWaiting(() => actor.GetVisitorProperties().BlacklistShop(task.ShopID));

            this.AddFinishAction(() => shop.RemoveCustomer(actor));

            yield return BehaviorHelper.SetTarget(Counter, counter);
            yield return BehaviorHelper.MoveTo(Counter);
            yield return BehaviorHelper.SetTarget(Item, item);
            yield return BehaviorHelper.CarryFromInventory(Item);
            yield return BehaviorReserve.Reserve(this, Item);
            // TODO place item on counter first? or wait for money on counter and then swap?
            // WTF if i place item on counter first, then i'll have to retrieve it in case of failure
            yield return new BehaviorResolveInteraction(() => (actor.Map, counterSurface), () => new UseHauledOnTarget());
            yield return BehaviorHelper.WaitForItem(Money, counterSurface, o => o.Def == ItemDefOf.Coins && o.StackSize >= cost);
            yield return new BehaviorResolveInteraction(Money, () => null);// new InteractionHaul());
            // TODO retrieve item if behavior fails while waiting (if going with placing the item on the counter before the money)
            yield return new BehaviorResolveInteraction(() => new InteractionStoreHauled());
        }
    }
}
