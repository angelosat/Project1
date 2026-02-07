using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Labors;
using Project1.Core.Towns;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Towns.Farming.Harvesting
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
