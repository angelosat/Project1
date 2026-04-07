using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Duties;

namespace Project1.Core.Towns.Farming
{
    class PlannerHarvesting : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            //if (!actor.HasDuty(DutyDefOf.Harvester))
            //    return null;
            if (actor.IsHauling)
                return null;
            var map = actor.Map;
            var manager = map.Town.GrowingManager;
            foreach(var plant in manager.GetHarvestablePlants())
            {
                if (!actor.CanReachAndReserve(plant))
                    continue;
                return new Plan(PlanDefOf.Harvesting, new InteractionTarget(plant));
            }
            return null;
        }
    }
}
