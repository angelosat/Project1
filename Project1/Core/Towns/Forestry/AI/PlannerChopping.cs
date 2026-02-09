using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors.Reserve;
using Project1.Core.AI.Labors;
using Project1.Core.Towns;
using Project1.Core.Towns.Designations;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using System.Linq;

namespace Project1.Core.Towns.Forestry.AI
{
    class PlannerChopping : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Lumberjack))
                return null;

            var manager = actor.Map.Town.DesignationManager;
            var trees = manager.GetDesignations(DesignationDefOf.Chop)
                .Where(o => actor.CanReserve(o))
                .OrderByReachableRegionDistance(actor);

            if (trees.FirstOrDefault() is not TargetArgs tree)
                return null;

            return new Plan(PlanDefOf.Chop) { TargetA = tree, Designation = DesignationDefOf.Chop };
        }
    }
}
