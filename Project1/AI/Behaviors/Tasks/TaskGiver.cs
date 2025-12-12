using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors.ItemOwnership;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    abstract public class TaskGiver
    {
        public static readonly TaskGiver Idle = new TaskGiverIdle();

        static readonly public List<TaskGiver> UrgentTaskGivers = [new TaskGiverSmartEquip()];

        static readonly public List<TaskGiver> EssentialTaskGivers = new()
        {
            new TaskGiverLeaveUnstandableCell(),
            new TaskGiverItemOwnership(),
            new TaskGiverConsiderItems(),

            //new TaskGiverIdle(),
        };

        static readonly public List<TaskGiver> CitizenTaskGivers = new()
        {
            new TaskGiverConstructing(),
            new TaskGiverRefueling(),
            new TaskGiverSwitchToggle(),
            new TaskGiverChopping(),
            new TaskGiverForaging(),
            new TaskGiverDigging(),
            new TaskGiverDeconstruct(),
            new TaskGiverTilling(),
            new TaskGiverPlanting(),
            new TaskGiverHarvesting(),
            new TaskGiverCrafting(),
            new TaskGiverHaulToStockpile(),
            new TaskGiverTradingOverCounter(),
            new TaskGiverOfferQuest(),
            new TaskGiverWorkplace()
        };

        static readonly public List<TaskGiver> VisitorTaskGivers = new()
        { 
            new TaskGiverVisitorRentRoom(),
            new TaskGiverBeTalkedTo(),
            new TaskGiverQuestComplete(),
            new TaskGiverGetQuests(),
            new TaskGiverTavernCustomer(),
            new TaskGiverDepart()
        };

        protected virtual AITask TryAssignTask(Actor actor) { return null; }
        public AITask FindTaskNew(Actor actor)
        {
            return TryAssignTask(actor);
        }
        public TaskGiverResult FindTask(Actor actor)
        {
            var task = TryAssignTask(actor);
            return task != null ? new TaskGiverResult(task, this) : TaskGiverResult.Empty;
        }
        public static void FindTool(Actor actor, AITask task, JobDef job)
        {
            task.Tool = FindTool(actor, job);
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
        
        public virtual AITask TryTaskOn(Actor actor, TargetArgs target, bool ignoreOtherReservations = false) { return null; }
        public virtual TaskDef CanGiveTask(Actor actor, TargetArgs target) { return null; }
    }
}
