namespace Start_a_Town_
{
    class TaskGiverWorkplace : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            return actor.Workplace?.GetTask(actor);
        }
    }
}
