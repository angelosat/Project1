using Project1.Framework.Entities.Actors;

namespace Start_a_Town_
{
    class TaskGiverSwitchToggle : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var sites = actor.Map.Town.DesignationManager.GetDesignations(DesignationDefOf.Switch);

            foreach (var site in sites)
            {
                var target = site;
                if (!actor.CanReserve(target) ||
                    !actor.CanReach(target))
                    continue;

                var task = new Plan(typeof(TaskBehaviorSwitchToggle), target);// new TargetArgs(actor.Map, target));
                return task;
            }

            return null;
        }
    }
}
