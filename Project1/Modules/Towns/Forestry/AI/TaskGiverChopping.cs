using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverChopping : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Lumberjack))
                return null;
            //var list = this.ChoppingTasks.Select(id => this.Town.Map.Net.GetNetworkEntity(id)).ToList();

            //var manager = actor.Map.Town.ChoppingManager;
            //var trees = manager.GetTrees()
            //    .Where(o => actor.CanReserve(o))
            //    .OrderByReachableRegionDistance(actor);

            var manager = actor.Map.Town.DesignationManager;
            var trees = manager.GetDesignations(DesignationDefOf.Chop)
                .Where(o => actor.CanReserve(o))
                .OrderByReachableRegionDistance(actor);

            if (!trees.Any())
                return null;

            /// why have i put this here?
            /// did i put it so that actor doesn't unequip tool between same consecutive tasks?
            //if (!trees.Any())
            //  return TaskHelper.TryStoreEquipped(actor, GearType.Mainhand); 

            var task = new AITask(TaskDefOf.Chopping);
            //FindTool(actor, task, JobDefOf.Lumberjack);
            task.SetEquipContextTargetIndex(TargetIndex.A); 
            task.TargetA = new TargetArgs(trees.First());
            return task;
        }
    }
}
