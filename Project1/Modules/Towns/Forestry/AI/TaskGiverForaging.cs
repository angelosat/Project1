using Project1.Core.Towns;
using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;
using Project1.Framework.Pathing;
using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverForaging : Planner
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
            var task = new Plan(typeof(TaskBehaviorHarvestingNew)) { TargetA = new TargetArgs(plant) };
            return task;
        }
    }
}
