using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverBuy : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            // check available money
            var money = actor.Inventory.Count(ItemDefOf.Coins);
            if (money == 0)
                return null;

            // find things for sale
            var shops = actor.Town.ShopManager.GetShops().OfType<Shop>();
            foreach(var shop in shops)
            {
                var items = shop.GetItemsForSale(o => IsForSale(o) && money >= o.GetValueTotal());
                var item = DecideItem(actor, items);
                if (item == null)
                    continue;

                var worker = shop.GetWorkers().FirstOrDefault();
                if (worker == null)
                    continue; // TODO maybe attempt to buy an item but fail the transaction and drop town approval rating if there's no worker ?

                var cost = item.GetValueTotal();
                if (!worker.InitiateTrade(actor, item, cost))
                    continue;
                return new AITask(typeof(TaskBehaviorBuy), new TargetArgs(item), new TargetArgs(worker));
            }

            return null;
        }

        static private bool IsForSale(GameObject o)
        {
            if (o.Def.Category == null)
                return false;
            return true;
        }
        static Entity DecideItem(Actor actor, IEnumerable<Entity> items)
        {
            var scores = items
                .Select(i => (i, actor.EvaluateItem(i)))
                .Where(p => p.Item2 > 0)
                .OrderByDescending(p => p.Item2)
                .ToList();

            var item = scores.FirstOrDefault();
            return item.Item1;
        }
    }
}
