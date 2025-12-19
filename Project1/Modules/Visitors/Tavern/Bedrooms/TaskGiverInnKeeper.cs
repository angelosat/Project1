namespace Start_a_Town_
{
    class TaskGiverInnKeeper : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var workplace = actor.Workplace as Tavern;
            var customers = workplace.Customers;
            foreach (var customer in customers.ToArray())
            {
                if (customer.Bedroom is not null && 
                    !customer.Customer.Possessions.Owns(customer.Bedroom))
                {
                    return new Plan(typeof(TaskBehaviorInnKeeper), customer.Customer);
                }
            }
            return null;
        }
    }
}
