using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverForaging : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Forager))
                return null;
            //var manager = actor.Map.Town.ChoppingManager;
            //var plants = manager.GetPlants()
            var plants = actor.Map.Town.DesignationManager
                .GetDesignations(DesignationDefOf.Harvest)
                .Where(o => actor.CanReserve(o))
                .OrderByReachableRegionDistance(actor);
            var plant = plants.FirstOrDefault();
            if (plant == null)
                return null;
            var task = new AITask(typeof(TaskBehaviorHarvestingNew)) { TargetA = new TargetArgs(plant) };
            return task;
        }
    }
}
