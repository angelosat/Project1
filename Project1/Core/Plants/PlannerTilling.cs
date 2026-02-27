using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Duties;
using System.Linq;

namespace Project1.Core.Plants
{
    class PlannerTilling : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.HasJob(DutyDefOf.Farmer))
                return null;
            foreach(var pos in actor.Map.Town.GrowingManager.GetNextTillingPos().Where(actor.CanReachAndReserve))
                return new Plan(PlanDefOf.Till, new TargetArgs(actor.Map, pos));
        
            return null;
        }
    }
}
