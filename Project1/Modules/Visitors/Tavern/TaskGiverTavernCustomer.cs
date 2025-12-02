using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverTavernCustomer : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var town = actor.Map.Town;
            var taverns = town.GetBusinesses<Tavern>();
            var favs = actor.Personality.GetFavorites().ToList();
            var budget = actor.GetMoneyTotal();
            var visitor = actor.GetVisitorProperties();
            foreach (var tavern in taverns)
            {
                if (visitor.HasRecentlyVisited(tavern))
                    continue;
                if (!tavern.TryGetAvailableTable(out var table))
                    continue;
                if(!TrySelectOrder(favs, tavern, out var order))
                    continue;
                var desiredIngredients = SelectIngredients(town.Map.World.Random, favs, order);
                var request = new VisitorCraftRequest(order, desiredIngredients);
                tavern.AddCustomer(actor, table, request);
                return new AITask(typeof(TaskBehaviorTavernCustomer), new TargetArgs(actor.Map, table)) { ShopID = tavern.ID };
            }
            return null;
        }
        bool TrySelectOrder(List<MaterialDef> favs, Tavern tavern, out CraftOrder order)
        {
            order = SelectOrder(favs, tavern);
            return order != null;
        }
        CraftOrder SelectOrder(List<MaterialDef> favs, Tavern tavern)
        {
            var orders = tavern.GetAvailableOrders().ToList();
            if (!orders.Any())
                return null;
            return orders.SelectRandomWeighted(tavern.Town.Map.Random, o => favs.Count(o.IsAllowed));
        }
        IEnumerable<(string reagent, ItemDef itemDef, MaterialDef material)> SelectIngredients(Random rand, List<MaterialDef> favs, CraftOrder order)
        {
            foreach (var r in order.Reaction.Reagents)
            {
                var restr = order.Restrictions[r.Name];
                var combos =
                    r.Ingredient.GetAllValidItemDefs()
                                .Where(i => !restr.IsRestricted(i))
                                .SelectMany(itemDef => itemDef.GetValidMaterials()
                                                              .Where(m => !restr.IsRestricted(m))
                                                              .Select(material => (itemDef, material)));
                var selectedCombo = combos.SelectRandomWeighted(rand, k => favs.Contains(k.material) ? 1 : 0);
                yield return (r.Name, selectedCombo.itemDef, selectedCombo.material);
            }
            yield break;
        }
        (CraftOrder order, IEnumerable<(string reagent, ItemDef itemDef, MaterialDef material)>) SelectOrderIngredients(ICollection<CraftOrder> orders, Random rand, int budget, List<MaterialDef> favs)
        {
            var ingredients = new List<(string reagent, ItemDef itemDef, MaterialDef material)>();
            foreach(var order in orders.Shuffle(rand))
            {

            }
            return (null, null);
        }
    }
}
