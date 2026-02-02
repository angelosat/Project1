using Project1.Framework.Skills;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public sealed class JobDefOf
    {
        static public readonly JobDef Digger = new JobDef("Digger", PlannerDefOf.Digging).SetTool(ToolUseDefOf.Digging);
        static public readonly JobDef Miner = new JobDef("Miner").SetTool(ToolUseDefOf.Mining);
        //static public readonly JobDef Hauler = new("Hauler", new TaskGiverRefueling(), new TaskGiverHaulToStockpile());
        static public readonly JobDef Hauler = new("Hauler", PlannerDefOf.Refueling, PlannerDefOf.Hauling);
        static public readonly JobDef Lumberjack = new JobDef("Lumberjack", PlannerDefOf.Chopping).SetTool(ToolUseDefOf.Chopping);
        static public readonly JobDef Forester = new("Forester");
        static public readonly JobDef Craftsman = new JobDef("Craftsman", PlannerDefOf.Crafting).SetTool(ToolUseDefOf.Building);
        static public readonly JobDef Smelter = new("Smelter");
        static public readonly JobDef Farmer = new JobDef("Farmer", PlannerDefOf.Tilling, PlannerDefOf.Sowing, PlannerDefOf.Harvesting).SetTool(ToolUseDefOf.Argiculture);
        static public readonly JobDef Harvester = new("Harvester");
        static public readonly JobDef Forager = new("Forager", PlannerDefOf.Foraging);
        static public readonly JobDef Builder = new JobDef("Builder", PlannerDefOf.Deconstructing, PlannerDefOf.Building).SetTool(ToolUseDefOf.Building);
        static public readonly JobDef Carpenter = new JobDef("Carpenter").SetTool(ToolUseDefOf.Carpentry);
        static public readonly JobDef Cook = new("Cook");
        static public readonly JobDef Guide = new("Guide");
        static public readonly JobDef QuestGiver = new("QuestGiver", PlannerDefOf.QuestGiving);
        static public readonly JobDef MiscDuties = new("MiscDuties", PlannerDefOf.Switching);
        static public readonly JobDef Workplace = new("TavernWorker", PlannerDefOf.Workplace);
        static JobDefOf()
        {
            foreach (var d in All)
                Def.Register(d);
        }
        static public readonly HashSet<JobDef> All = new()
                {
                    Workplace,
                    Digger,
                    Miner,
                    Lumberjack,
                    Forester,
                    Craftsman,
                    Smelter,
                    Farmer,
                    Harvester,
                    Forager,
                    Builder,
                    Carpenter,
                    Cook,
                    Guide,
                    QuestGiver,
                    Hauler,
                    MiscDuties,
                };
    }
}