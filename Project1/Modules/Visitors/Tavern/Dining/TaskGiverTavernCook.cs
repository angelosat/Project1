using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverTavernCook : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var tavern = actor.Workplace as Tavern;
            var map = actor.Map;
            var customers = tavern.Customers;
            foreach (var customer in customers.ToArray())
            {
                if (customer.IsSeated && customer.IsOrderTaken && customer.Dish == null) // TODO && order ready/not ready
                {
                    var availableKitchen = tavern.GetAvailableKitchen();
                    if (availableKitchen.HasValue)
                    {
                        var order = customer.CraftRequest;
                        var allObjects = map.GetEntities().OfType<Entity>();
                        List<(string reagent, Entity item)> foundIngredients = new();
                        foreach (var (reagentName, item, material) in order.GetPreferences())
                        {
                            var foundItem = allObjects.FirstOrDefault(o => o.Def == item && o.PrimaryMaterial == material && actor.CanReserve(o));
                            if (foundItem == null)
                                return null; // TODO start delivering materials even if not all of them are currently available?
                            foundIngredients.Add((reagentName, foundItem));
                        }
                        var task = new AITask(typeof(TaskBehaviorTavernWorkerPrepareOrder));
                        foreach (var (reagent, item) in foundIngredients)
                            task.AddTarget(TargetIndex.A, item, 1);
                        task.SetTarget(TargetIndex.B, availableKitchen.Value.At(map));
                        task.IngredientsUsed = foundIngredients.ToDictionary(i => i.reagent, i => new ObjectRefIDsAmount(i.item, 1));
                        task.Order = order.Order;
                        task.CustomerProps = customer;
                        return task;
                    }
                }
            }
            return null;
        }
    }
}
