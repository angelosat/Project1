namespace Start_a_Town_.AI.Behaviors
{
    class TaskGiverLeaveUnstandableCell : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var cell = actor.Global.ToCell();
            var map = actor.Map;
            if (map.IsStandableIn(cell))
                return null;
            var iterator = cell.GetRadial();
            foreach(var pos in iterator)
            {
                if (!map.IsStandableIn(pos))
                    continue;
                var task = new AITask(TaskDefOf.Moving, new TargetArgs(pos));
                return task;
            }
            return null;
        }
    }
}
