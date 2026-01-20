using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors.ItemOwnership;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    abstract public class Planner
    {
        static readonly public Planner Idle = PlannerDefOf.Idle.Worker;// new TaskGiverIdle();

        static readonly public List<Planner> UrgentPlanners = [PlannerDefOf.SmartEquip.Worker];// [new SmartEquipPlanner()];

        static readonly public List<Planner> EssentialPlanners =
        [
            new TaskGiverLeaveUnstandableCell(),
            new TaskGiverItemOwnership(),
            PlannerDefOf.Inventory.Worker
            //new TaskGiverIdle(),
        ];

        static readonly public List<Planner> CitizenTaskGivers =
        [
            //new BuildingPlanner(),
            PlannerDefOf.Building.Worker,
            new TaskGiverRefueling(),
            new TaskGiverSwitchToggle(),
            //new ChoppingPlanner(),
            PlannerDefOf.Chopping.Worker,
            new TaskGiverForaging(),
            //new DiggingPlanner(),
            PlannerDefOf.Digging.Worker,
            new TaskGiverDeconstruct(),
            //new TillingPlanner(),
            PlannerDefOf.Tilling.Worker,
            //new SowingPlanner(),
            PlannerDefOf.Sowing.Worker,
            new TaskGiverHarvesting(),
            //new CraftingPlanner(),
            PlannerDefOf.Crafting.Worker,
            //new TaskGiverHaulToStockpile(),
            new TaskGiverTradingOverCounter(),
            new TaskGiverOfferQuest(),
            new TaskGiverWorkplace()
        ];

        static readonly public List<Planner> VisitorPlanners =
        [
            new TaskGiverVisitorRentRoom(),
            new TaskGiverBeTalkedTo(),
            new TaskGiverQuestComplete(),
            new TaskGiverGetQuests(),
            new TaskGiverTavernCustomer(),
            //new DeparturePlanner(),
            PlannerDefOf.Departure.Worker
        ];

        protected virtual Plan TryPlan(Actor actor) { return null; }
        public Plan FindPlanNew(Actor actor)
        {
            return TryPlan(actor);
        }
        public PlannerResult FindPlan(Actor actor)
        {
            var task = TryPlan(actor);
            return task != null ? new PlannerResult(task, this) : PlannerResult.Empty;
        }
        public static void FindTool(Actor actor, Plan task, JobDef job)
        {
            //task.Tool = FindTool(actor, job);
        }
        public static TargetArgs FindTool(Actor actor, JobDef job)
        {
            var preference = actor.ItemPreferences.GetPreference(job);
            var equipped = actor.GetEquipmentSlot(GearType.Mainhand);//.Object;
            if (preference is not null && (equipped == preference || actor.Inventory.Contains(preference)))
                return preference;
            if (equipped != null && equipped.ProvidesSkill(job.ToolUse))
                return new TargetArgs(equipped);
            else
                return TaskHelper.FindItemAnywhere(actor, o => o is Tool tool && tool.ProvidesSkill(job.ToolUse));
        }
        
        public virtual Plan TryTaskOn(Actor actor, TargetArgs target, bool ignoreOtherReservations = false) { return null; }
        public virtual PlanDef CanGiveTask(Actor actor, TargetArgs target) { return null; }
    }
}
