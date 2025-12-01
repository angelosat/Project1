using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorAcceptSellOverCounter : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            // TODO wait for price negotiation
            // TODO send reply / complete transaction
            var actor = this.Actor;
            var state = this.Actor.GetState();
            var task = this.Task;
            var shop = actor.Town.GetShop(task.ShopID) as Shop;

            yield return new BehaviorCustom()
            {
                InitAction = () =>
                {
                    task.SetTarget(TargetIndex.A, shop.Counter.Value.At(actor.Map));
                    task.SetTarget(TargetIndex.B, shop.GetNextSaleItem());
                }
            };
            yield return new BehaviorGetAtNewNew(TargetIndex.A);
            yield return new BehaviorWait(() => task.TargetB.Object.Parent == null && task.TargetB.Object.Global.ToCell() == task.TargetA.Global.Above());
            yield return new BehaviorInteractionNew(TargetIndex.B, () => new InteractionHaul());
            // TODO wait until money on counter
            yield return new BehaviorWait(() =>
            {
                var itemOnCounter = actor.Map.GetObjects(task.TargetA.Global.Above());
                var money = itemOnCounter.FirstOrDefault(i => i.Def == ItemDefOf.Coins);
                if (money == null)
                    return false;
                task.SetTarget(TargetIndex.C, money);
                return true;
            });
            yield return new BehaviorInteractionNew(() => task.TargetC, () => new InteractionSwapCarried());

            // if carrying coins, store in inventory. otherwise drop or haul to stockpile
            yield return new BehaviorInteractionNew(TargetIndex.A, () =>
            {
                if (this.Actor.Hauled.Def == ItemDefOf.Coins)
                    return new InteractionStoreHauled();
                else
                    return new InteractionThrow();
            });
            // UNDONE i remove customer on the buyer's behavior finish action
            //yield return new BehaviorWait(() =>
            //{
            //    var item = task.TargetB.Object as Entity;
            //    var customer = shop.GetNextCustomer();
            //    if (item.Parent == customer)
            //    {
            //        shop.RemoveCustomer(customer);
            //        return true;
            //    }
            //    return false;
            //});
        }
    }
}
