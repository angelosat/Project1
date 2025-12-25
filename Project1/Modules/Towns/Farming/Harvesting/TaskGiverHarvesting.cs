namespace Start_a_Town_
{
    class TaskGiverHarvesting : Planner
    {
        protected override Plan TryPlan(Actor actor)
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
                    if (plant.PlantComponent.Species.ProducesFruit)
                    {
                        var task = new Plan(PlanDefOf.Harvesting, new TargetArgs(plant));
                        return task;
                    }
                    else
                    {
                        var task = new Plan(PlanDefOf.Chopping, plant)
                        {
                            Tool = FindTool(actor, JobDefOf.Lumberjack)
                        };
                        return task;
                    }
                }
            }
            return null;
        }
    }
}
