using Project1.Core.AI.Planners;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Inns;
using System.Collections.Generic;

namespace Project1.Core.AI.Behaviors
{
    abstract public class Planner
    {
        static readonly public Planner Idle = PlannerDefOf.Idle.Worker;// new TaskGiverIdle();

        static readonly public List<PlannerDef> UrgentPlanners = [PlannerDefOf.SmartEquip];// [new SmartEquipPlanner()];

        static readonly public List<PlannerDef> EssentialPlanners =
        [
            PlannerDefOf.Inventory
        ];

        static readonly public List<PlannerDef> CitizenTaskGivers =
        [
            PlannerDefOf.Building,
            PlannerDefOf.Refueling,
            PlannerDefOf.Switching,
            PlannerDefOf.Chopping,
            PlannerDefOf.Foraging,
            PlannerDefOf.Digging,
            PlannerDefOf.Deconstructing,
            PlannerDefOf.Tilling,
            PlannerDefOf.Sowing,
            PlannerDefOf.Harvesting,
            PlannerDefOf.Crafting,
            PlannerDefOf.QuestGiving,
            PlannerDefOf.Workplace,
        ];

        static readonly public List<PlannerDef> VisitorPlanners =
        [
            PlannerDefOf.LodgingCheckin,
            PlannerDefOf.Buy,
            PlannerDefOf.Browse,
            //PlannerDefOf.Departure
        ];

        protected abstract Plan TryPlan(Actor actor);
            //=> null;

        public Plan FindPlanNew(Actor actor)
            => TryPlan(actor);
        
        public PlannerResult FindPlan(Actor actor)
        {
            var task = TryPlan(actor);
            task?.Actor = actor;
            return task != null ? new PlannerResult(task, this) : PlannerResult.Empty;
        }
        public virtual bool ShouldContinue(Actor actor, Plan plan) => true;
        public virtual Plan TryTaskOn(Actor actor, TargetArgs target, bool ignoreOtherReservations = false) => null;
        public virtual PlanDef CanGiveTask(Actor actor, TargetArgs target) => null; 
    }
}
