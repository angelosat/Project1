using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverSellOverCounter : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var shops = actor.Town.ShopManager.GetShops().OfType<Shop>();

            var prefs = actor.ItemPreferences;
            var junk = prefs.GetJunk();
            foreach (var item in junk)
            {
                foreach (var shop in shops)
                {
                    // TODO find a shop worker to sell to
                    // TODO only return a task if there's an available buyer with enough money?
                    // TODO try to sell anyway and drop town rating if unsuccessful?
                    if (!shop.IsValid())
                        continue;
                    if (actor.GetVisitorProperties().IsBlacklisted(shop))
                        continue;
                    

                    var cost = item.GetValueTotal();

                    if (!shop.RequestTransactionSell(actor, item, cost, out var transaction))
                        continue;
                    return new AITask(typeof(TaskBehaviorSellOverCounter), item) { ShopID = shop.ID, Transaction = transaction };
                }
            }
            return null;
        }
    }
}
