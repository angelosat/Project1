using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Designations;
using Project1.Framework;

namespace Project1.Core.Towns.Digging
{
    class PlannerDigging : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            //if (!actor.HasDuty(DutyDefOf.Digger))
            //    return null;
            var jobs = actor.Map.Town.DesignationManager.GetDesignationTargets(DesignationDefOf.Mine);

            if (actor.IsHauling)
                return null;
            foreach (var target in jobs) // TODO: check if another npc is standing on the target block to be digged
            {
                var pos = (IntVec3)target.Global;
   
                if (!actor.CanReachAndReserve(pos))
                    continue;

                // TODO branch here, decide which plan to use based on block material (mine/dig/chop)

                var task = new Plan(PlanDefOf.Digging, target) { Designation = DesignationDefOf.Mine };// new TargetArgs(actor.Map, target));

                return task;
            }
            return null;
        }
    }
}
