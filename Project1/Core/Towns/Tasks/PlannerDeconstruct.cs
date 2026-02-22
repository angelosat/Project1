using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Labors;
using Project1.Core.AI.Reservations;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Designations;
using Project1.Framework;

namespace Project1.Core.Towns.Tasks
{
    class PlannerDeconstruct : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Builder))
                return null;
            //var allPositions = actor.Map.Town.DesignationManager.GetDesignations(DesignationDefOf.Deconstruct);
            var targets = actor.Map.Town.DesignationManager.GetDesignationTargets(DesignationDefOf.Deconstruct);
            foreach(var target in targets)
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
                //FindTool(actor, task, JobDefOf.Builder);
                return task;
            }
            return null;   
        }
    }
}
