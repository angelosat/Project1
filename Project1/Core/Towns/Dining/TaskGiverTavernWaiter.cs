using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Towns
{
    class TaskGiverTavernWaiter : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var workplace = actor.Workplace as Tavern;
            var customers = workplace.Customers;
            foreach (var customer in customers.ToArray())
            {
                if (customer.IsSeated && !customer.IsOrderTaken)
                {
                    return new Plan(typeof(TaskBehaviorTavernWorkerTakeOrder), customer.Customer);
                }
                else if (!customer.IsServed && customer.Dish != null)
                {
                    customer.ServedBy = actor;
                    return new Plan(typeof(TaskBehaviorTavernWorkerServe), customer.Dish, (actor.Map, customer.Table.Above)) { CustomerID = customer.CustomerID };
                }
            }
            return null;
        }
    }
}
