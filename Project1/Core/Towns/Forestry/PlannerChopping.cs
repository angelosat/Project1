using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Reservations;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Designations;
using Project1.Core.Towns.Duties;
using System.Linq;

namespace Project1.Core.Towns.Forestry
{
    class PlannerChopping : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.HasDuty(DutyDefOf.Lumberjack))
                return null;
            if (actor.IsHauling)
                return null;
            var manager = actor.Map.Town.DesignationManager;
            //var trees = manager.GetDesignations(DesignationDefOf.Chop)
            var trees = manager.GetDesignationTargets(DesignationDefOf.Chop)
                .Where(o => actor.CanReserve(o))
                .OrderByReachableRegionDistance(actor);

            if (trees.FirstOrDefault() is not TargetArgs tree)
                return null;

            return new Plan(PlanDefOf.Chop) { TargetA = tree, Designation = DesignationDefOf.Chop };
        }
    }
}
