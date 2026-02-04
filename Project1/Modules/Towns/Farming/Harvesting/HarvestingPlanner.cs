using Project1.Core.Towns;
using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;

namespace Start_a_Town_
{
    class HarvestingPlanner : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Harvester))
                return null;
            var map = actor.Map;
            var manager = map.Town.GrowingManager;
            foreach(var plant in manager.GetHarvestablePlants())
            {
                if (!actor.CanReachAndReserve(plant))
                    continue;
                return new Plan(PlanDefOf.Harvesting, new TargetArgs(plant));
            }
            return null;
        }
    }
}
