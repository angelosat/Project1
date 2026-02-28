using System.Linq;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.Towns.Designations;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Farming;
using Project1.Core.AI.Reservations;
using Project1.Core.Towns.Duties;

namespace Project1.Core.Towns.Forestry
{
    class PlannerForaging : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            //if (!actor.HasDuty(DutyDefOf.Forager))
            //    return null;
            var plants = actor.Map.Town.DesignationManager
                .GetDesignationTargets(DesignationDefOf.Harvest)
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
