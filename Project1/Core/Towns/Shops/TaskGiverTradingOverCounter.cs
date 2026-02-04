using Project1.Framework.Entities.Actors;
using Start_a_Town_;
using System;

namespace Project1.Core.Towns.Shops
{
    class TaskGiverTradingOverCounter : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var shop = actor.Town.ShopManager.GetShop<Shop>(actor);
            if (shop == null)
                return null;
            if (!shop.IsValid())
                return null;
            if (!shop.TryGetNextTransaction(out var transaction))
                return null;
            if (!shop.CanExecuteTransaction(actor, transaction))
                return null;
            if (transaction.Type == Transaction.Types.Buy)
                return new Plan(typeof(TaskBehaviorAcceptSellOverCounter), (actor.Map, shop.Counter.Value));
            else if (transaction.Type == Transaction.Types.Sell)
                return new Plan(typeof(TaskBehaviorAcceptBuyOverCounter)) { ShopID = shop.ID, Transaction = transaction }; // shop holds value for counter so no need to pass it to the task as a target
            else
                throw new Exception();
        }
    }
}
