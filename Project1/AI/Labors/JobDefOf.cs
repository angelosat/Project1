using System.Collections.Generic;

namespace Start_a_Town_
{
    public sealed class JobDefOf
    {
        static public readonly JobDef Digger = new JobDef("Digger", new TaskGiverDigging()).SetTool(ToolUseDefOf.Digging);
        static public readonly JobDef Miner = new JobDef("Miner").SetTool(ToolUseDefOf.Mining);
        static public readonly JobDef Hauler = new("Hauler", new TaskGiverRefueling(), new TaskGiverHaulToStockpile());
        static public readonly JobDef Lumberjack = new JobDef("Lumberjack", new TaskGiverChopping()).SetTool(ToolUseDefOf.Chopping);
        static public readonly JobDef Forester = new("Forester");
        static public readonly JobDef Craftsman = new JobDef("Craftsman", new TaskGiverCrafting()).SetTool(ToolUseDefOf.Building);
        static public readonly JobDef Smelter = new("Smelter");
        static public readonly JobDef Farmer = new JobDef("Farmer", new TaskGiverTilling(), new TaskGiverPlanting(), new TaskGiverHarvesting()).SetTool(ToolUseDefOf.Argiculture);
        static public readonly JobDef Harvester = new("Harvester");
        static public readonly JobDef Forager = new("Forager", new TaskGiverForaging());
        static public readonly JobDef Builder = new JobDef("Builder", new TaskGiverDeconstruct(), new TaskGiverConstructing()).SetTool(ToolUseDefOf.Building);
        static public readonly JobDef Carpenter = new JobDef("Carpenter").SetTool(ToolUseDefOf.Carpentry);
        static public readonly JobDef Cook = new("Cook");
        static public readonly JobDef Guide = new("Guide");
        static public readonly JobDef QuestGiver = new("QuestGiver", new TaskGiverOfferQuest());
        static public readonly JobDef MiscDuties = new("MiscDuties", new TaskGiverSwitchToggle());
        static public readonly JobDef Workplace = new("TavernWorker", new TaskGiverWorkplace());
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