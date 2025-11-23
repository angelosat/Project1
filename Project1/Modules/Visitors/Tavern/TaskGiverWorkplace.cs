namespace Start_a_Town_
{
    class TaskGiverWorkplace : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            return actor.Workplace?.GetTask(actor);
        }
    }
}
