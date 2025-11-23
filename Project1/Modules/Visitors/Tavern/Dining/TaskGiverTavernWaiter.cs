namespace Start_a_Town_
{
    class TaskGiverTavernWaiter : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var workplace = actor.Workplace as Tavern;
            var customers = workplace.Customers;
            foreach (var customer in customers.ToArray())
            {
                if (customer.IsSeated && !customer.IsOrderTaken)
                {
                    return new AITask(typeof(TaskBehaviorTavernWorkerTakeOrder), customer.Customer);
                }
                else if (!customer.IsServed && customer.Dish != null)
                {
                    customer.ServedBy = actor;
                    return new AITask(typeof(TaskBehaviorTavernWorkerServe), customer.Dish, (actor.Map, customer.Table.Above)) { CustomerID = customer.CustomerID };
                }
            }
            return null;
        }
    }
}
