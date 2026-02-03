using Project1.Framework.Pathing;
using System.Linq;

namespace Start_a_Town_
{
    class ChoppingPlanner : Planner
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
