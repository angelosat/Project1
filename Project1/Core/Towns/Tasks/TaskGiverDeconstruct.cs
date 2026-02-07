using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Reserve;
using Project1.Core.AI.Labors;
using Project1.Core.Towns;
using Project1.Core.Towns.Designations;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Towns.Tasks
{
    class TaskGiverDeconstruct : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Builder))
                return null;
            var allPositions = actor.Map.Town.DesignationManager.GetDesignations(DesignationDefOf.Deconstruct);
            foreach(var target in allPositions)
            {
                var pos = (IntVec3)target.Global;
                if (!actor.CanReserve(target))
                    continue;
                if (!actor.CanReach(target))
                    continue;
                if (!actor.Map.IsCellEmptyNew(pos.Above))
                    continue;
                var task = new Plan()
                {
                    BehaviorType = typeof(TaskBehaviorDeconstruct),
                };
                task.SetTarget(TaskBehaviorDeconstruct.DeconstructInd, target);// new TargetArgs(actor.Map, target));
                FindTool(actor, task, JobDefOf.Builder);
                return task;
            }
            return null;   
        }
    }
}
