using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors.ItemOwnership;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    abstract public class Planner
    {
        public static readonly Planner Idle = new TaskGiverIdle();

        static readonly public List<Planner> UrgentPlanners = [new TaskGiverSmartEquip()];

        static readonly public List<Planner> EssentialPlanners = new()
        {
            new TaskGiverLeaveUnstandableCell(),
            new TaskGiverItemOwnership(),
            new EquippingPlanner(),

            //new TaskGiverIdle(),
        };

        static readonly public List<Planner> CitizenTaskGivers = new()
        {
            new BuildingPlanner(),
            new TaskGiverRefueling(),
            new TaskGiverSwitchToggle(),
            new ChoppingPlanner(),
            new TaskGiverForaging(),
            new TaskGiverDigging(),
            new TaskGiverDeconstruct(),
            new TillingPlanner(),
            new SowingPlanner(),
            new TaskGiverHarvesting(),
            new CraftingPlanner(),
            //new TaskGiverHaulToStockpile(),
            new TaskGiverTradingOverCounter(),
            new TaskGiverOfferQuest(),
            new TaskGiverWorkplace()
        };

        static readonly public List<Planner> VisitorPlanners = new()
        { 
            new TaskGiverVisitorRentRoom(),
            new TaskGiverBeTalkedTo(),
            new TaskGiverQuestComplete(),
            new TaskGiverGetQuests(),
            new TaskGiverTavernCustomer(),
            new DeparturePlanner()
        };

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
