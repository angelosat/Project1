using System.Linq;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors.Reserve;
using Project1.Core.AI.Labors;
using Project1.Core.Towns.Designations;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Farming;

namespace Project1.Core.Towns.Forestry
{
    class PlannerForaging : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Forager))
                return null;
            var plants = actor.Map.Town.DesignationManager
                .GetDesignations(DesignationDefOf.Harvest)
                .Where(o => actor.CanReserve(o))
                .OrderByReachableRegionDistance(actor);
            var plant = plants.FirstOrDefault();
            if (plant == null)
                return null;
            var task = new Plan(typeof(BehaviorHarvesting)) { TargetA = new TargetArgs(plant) };
            return task;
        }
    }
}
