using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;
using System.Linq;

namespace Project1.Core.Systems.Plants
{
    class PlannerTilling : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            //if (!actor.HasDuty(DutyDefOf.Farmer))
            //    return null;
            if (actor.IsHauling)
                return null;
            foreach(var pos in actor.Map.Town.GrowingManager.GetNextTillingPos().Where(actor.CanReachAndReserve))
                return new Plan(PlanDefOf.Till, new InteractionTarget(actor.Map, pos));
        
            return null;
        }
    }
}
