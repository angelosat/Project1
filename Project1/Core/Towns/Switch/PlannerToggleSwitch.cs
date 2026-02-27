using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Towns.Designations;
using Project1.Core.Entities.Actors;
using Project1.Core.AI.Reservations;

namespace Project1.Core.Towns.Switch
{
    class PlannerToggleSwitch : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var sites = actor.Map.Town.DesignationManager.GetDesignationTargets(DesignationDefOf.Switch);

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
