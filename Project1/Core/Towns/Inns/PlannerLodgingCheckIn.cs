using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using Project1.Core.Resources;
using System.Linq;

namespace Project1.Core.Towns.Inns
{
    internal sealed class PlannerLodgingRegisterGuest : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.IsTownMember)
                return null;
            if (actor.IsHauling)
                return null;
            var manager = actor.Map.Town.InnManager;
            var busyServicePoints = manager.GetServicePointsWithQueue();
            foreach(var point in busyServicePoints)
            {
                if (!manager.TryFindBedFrom(point, out var foundBed))
                    continue;
                return new Plan(InnsDefOf.PlanRegisterGuest, new TargetArgs(actor.Map, point));
            }
            return null;
        }
    }
    internal sealed class PlannerLodgingCheckIn : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (actor.IsTownMember)
                return null;
            if (actor.IsHauling)
                return null;
            if (actor.HasCheckedIn)
                return null;
            if (actor.Needs.GetPercentage(NeedDefOf.Energy) > .5f) // TODO: make it variable
                return null;
            if (actor.Resources.GetPercentage(ResourceDefOf.Patience) < .5f) // TODO: make it variable
                return null;
            var manager = actor.Map.Town.InnManager;
            var servicePoints = manager.GetServicePoints();
            if (!servicePoints.Any())
                return null;
            // TODO: prefer smaller queues
            if (!actor.TryChoosePosition(servicePoints, out var found))
                return null;

            return new Plan(InnsDefOf.PlanCheckIn, new TargetArgs(actor.Map, found));
        }
    }
}
