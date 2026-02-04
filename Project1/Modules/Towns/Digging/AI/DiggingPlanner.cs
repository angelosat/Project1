using Project1.Core.Gear;
using Project1.Core.Towns;
using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;
using System;

namespace Start_a_Town_
{
    class DiggingPlanner : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Digger))
                return null;
            var map = actor.Map;
            
            var jobs = actor.Map.Town.DesignationManager.GetDesignations(DesignationDefOf.Mine);

            var mainhand = actor.GetEquipmentSlot(GearTypeDefOf.Mainhand);

            /// why have i put this here?
            /// did i put it so that actor doesn't unequip tool between same consecutive tasks?
            //if (!jobs.Any())
            //    return TaskHelper.TryStoreEquipped(actor, GearType.Mainhand); // WHY DO THIS HERE? i clean up in behaviorhandletask

            foreach (var target in jobs) // TODO: check if another npc is standing on the target block to be digged
            {
                var pos = (IntVec3)target.Global;
                if (!actor.CanReserve(target))
                    continue;
                if (!actor.CanReach(pos))
                    continue;
           
                //if(TaskHelper.TryHaulAside(actor, pos.Above, out var haulAsideTask))
                //{
                //    if (haulAsideTask != null)
                //        return haulAsideTask;
                //}
                //else
                //    continue;

                var block = map.GetBlock(pos);
                var material = map.GetMaterial(pos);
                var skill = material.Type.JobToExtract;

                if (skill == null)
                    throw new Exception();

                var task = new Plan(PlanDefOf.Digging, target) { Designation = DesignationDefOf.Mine };// new TargetArgs(actor.Map, target));
                //FindTool(actor, task, skill);

                return task;
            }
            return null;
        }

        static public bool TryGetTask(Actor actor, TargetArgs target, out Plan task)
        {
            task = null;
            var global = target.Global;
            var block = target.Block;
            if (!block.IsMinable)
                return false;
            if (!actor.CanReserve(target))
                return false;
            if (!actor.CanReach(target))
                return false;
            var material = actor.Map.GetMaterial(global);
            var skill = material.Type.JobToExtract;

            if (skill == null)
                throw new Exception();
           
            task = new Plan()
            {
                BehaviorType = typeof(TaskBehaviorDigging),
            };
            task.SetTarget(TaskBehaviorDigging.MineInd, target);
            task.SetEquipContextTargetIndex(TaskBehaviorDigging.MineInd);
            FindTool(actor, task, skill);
            return true;
        }
    }
}
