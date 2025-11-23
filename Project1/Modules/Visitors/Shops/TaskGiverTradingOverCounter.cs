using System;

namespace Start_a_Town_
{
    class TaskGiverTradingOverCounter : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
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
                return new AITask(typeof(TaskBehaviorAcceptSellOverCounter), (actor.Map, shop.Counter.Value));
            else if (transaction.Type == Transaction.Types.Sell)
                return new AITask(typeof(TaskBehaviorAcceptBuyOverCounter)) { ShopID = shop.ID, Transaction = transaction }; // shop holds value for counter so no need to pass it to the task as a target
            else
                throw new Exception();
        }
    }
}
