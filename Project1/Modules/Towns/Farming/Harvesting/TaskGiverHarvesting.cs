namespace Start_a_Town_
{
    class TaskGiverHarvesting : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
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
                    if (plant.PlantComponent.PlantProperties.ProducesFruit)
                    {
                        var task = new AITask(TaskDefOf.Harvesting, new TargetArgs(plant));
                        return task;
                    }
                    else
                    {
                        var task = new AITask(TaskDefOf.Chopping, plant)
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
