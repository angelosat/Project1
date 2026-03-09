using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Towns.Designations;
using Project1.Core.Entities.Actors;
using System.Linq;

namespace Project1.Core.Towns.Switch
{
    class PlannerToggleSwitch : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            var targets = actor.Map.Town.DesignationManager.GetDesignationTargets(DesignationDefOf.Switch);
            targets = targets.Union(actor.Map.Town.DesignationManager.GetDesignationTargets(DesignationDefOf.SwitchOff));

            foreach (var target in targets)
            {
                if(!actor.CanReachAndReserve(target.Global))
                    continue;
                return new Plan(PlanDefOf.Switching, target);
            }

            return null;
        }
    }
}
