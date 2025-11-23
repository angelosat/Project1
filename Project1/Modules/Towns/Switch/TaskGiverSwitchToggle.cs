namespace Start_a_Town_
{
    class TaskGiverSwitchToggle : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var sites = actor.Map.Town.DesignationManager.GetDesignations(DesignationDefOf.Switch);

            foreach (var site in sites)
            {
                var global = site;
                if (!actor.CanReserve(global) ||
                    !actor.CanReach(global))
                    continue;

                var task = new AITask(typeof(TaskBehaviorSwitchToggle), new TargetArgs(actor.Map, global));
                return task;
            }

            return null;
        }
    }
}
