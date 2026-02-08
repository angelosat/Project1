using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Reserve;
using Project1.Core.AI.Labors;
using Project1.Core.Gear;
using Project1.Core.Towns.Designations;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using System;
using Project1.Framework.Math;

namespace Project1.Core.Towns.Digging.AI
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

            foreach (var target in jobs) // TODO: check if another npc is standing on the target block to be digged
            {
                var pos = (IntVec3)target.Global;
                if (!actor.CanReserve(target))
                    continue;
                if (!actor.CanReach(pos))
                    continue;

                var block = map.GetBlock(pos);
                var material = map.GetMaterial(pos);
                var skill = material.Type.JobToExtract;

                if (skill == null)
                    throw new Exception();

                var task = new Plan(PlanDefOf.Digging, target) { Designation = DesignationDefOf.Mine };// new TargetArgs(actor.Map, target));

                return task;
            }
            return null;
        }
    }
}
