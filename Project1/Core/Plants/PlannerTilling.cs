using System.Linq;
using Project1.Framework;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Reserve;
using Project1.Core.AI.Labors;
using Project1.Core.Towns;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Farming;

namespace Project1.Core.Plants
{
    class PlannerTilling : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Farmer))
                return null;
            foreach(var pos in actor.Map.Town.GrowingManager.GetNextTillingPos().Where(actor.CanReachAndReserve))
                return new Plan(PlanDefOf.Till, new TargetArgs(actor.Map, pos));
        
            return null;
        }
        protected Plan TryPlanOld(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Farmer))
                return null;
            var loc = GetBestTillingLocation(actor);
            if (!loc.HasValue)
                return null;
            var task = new Plan(PlanDefOf.Till);
            task.SetTarget(TaskBehaviorTilling.TargetInd, new TargetArgs(actor.Map, loc.Value));
            FindTool(actor, task, JobDefOf.Farmer);
            return task;
        }

        static IntVec3? GetBestTillingLocation(Actor actor)
        {
            var map = actor.Map;
            var reachableZones = map.Town.ZoneManager
                .GetZones<GrowingZone>()
                .Where(z => z.Tilling && z.GetTillingPositions() is var tilling && tilling.Any() && actor.CanReach(tilling.First()));
            var locs = reachableZones
                .SelectMany(z => z.GetTillingPositions())
                .Where(pos => actor.CanReserve(pos));
            if (!locs.Any())
                return null;
            return locs.First();
        }
    }
}
