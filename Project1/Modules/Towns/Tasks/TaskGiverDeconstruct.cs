namespace Start_a_Town_
{
    class TaskGiverDeconstruct : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Builder))
                return null;
            var allPositions = actor.Map.Town.DesignationManager.GetDesignations(DesignationDefOf.Deconstruct);
            foreach(var pos in allPositions)
            {
                if (!actor.CanReserve(pos))
                    continue;
                if (!actor.CanReach(pos))
                    continue;
                if (!actor.Map.IsCellEmptyNew(pos.Above))
                    continue;
                var task = new AITask()
                {
                    BehaviorType = typeof(TaskBehaviorDeconstruct),
                };
                task.SetTarget(TaskBehaviorDeconstruct.DeconstructInd, new TargetArgs(actor.Map, pos));
                FindTool(actor, task, JobDefOf.Builder);
                return task;
            }
            return null;   
        }
    }
}
