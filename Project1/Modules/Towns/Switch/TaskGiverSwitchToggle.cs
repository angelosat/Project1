namespace Start_a_Town_
{
    class TaskGiverSwitchToggle : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var sites = actor.Map.Town.DesignationManager.GetDesignations(DesignationDefOf.Switch);

            foreach (var site in sites)
            {
                var target = site;
                if (!actor.CanReserve(target) ||
                    !actor.CanReach(target))
                    continue;

                var task = new AITask(typeof(TaskBehaviorSwitchToggle), target);// new TargetArgs(actor.Map, target));
                return task;
            }

            return null;
        }
    }
}
