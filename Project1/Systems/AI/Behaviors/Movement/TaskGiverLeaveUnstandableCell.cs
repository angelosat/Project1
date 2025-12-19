namespace Start_a_Town_.AI.Behaviors
{
    class TaskGiverLeaveUnstandableCell : Planner
    {
        protected override Plan TryPlan(Actor actor)
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
                var task = new Plan(TaskDefOf.Moving, pos.At(map));
                return task;
            }
            return null;
        }
    }
}
