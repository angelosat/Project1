using Start_a_Town_.Components;

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
        protected Plan TryPlanOld(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Harvester))
                return null;
            var map = actor.Map;
            var zones = map.Town.ZoneManager.GetZones<GrowingZone>();
            foreach (var zone in zones)
            {
                if (!zone.Harvesting)
                    continue;
                var plants = zone.GetHarvestablePlantsLazy();
                foreach (var plant in plants)
                {
                    if (!actor.CanReserve(plant as GameObject) ||
                        !actor.CanReach(plant))
                        continue;
                    var comp = plant.GetComponent<PlantComponent>();
                    if (comp.Species.ProducesFruit)
                    {
                        var task = new Plan(PlanDefOf.Harvesting, new TargetArgs(plant));
                        return task;
                    }
                    else
                    {
                        var task = new Plan(PlanDefOf.Chop, plant)
                        //{
                        //    Tool = FindTool(actor, JobDefOf.Lumberjack)
                        //}
                        ;
                        return task;
                    }
                }
            }
            return null;
        }
    }
}
