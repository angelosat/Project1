using Project1.Core.AI.Planners;
using Project1.Core.Systems.Tools;
using Project1.Core.Towns.Services.Repairing;

namespace Project1.Core.Towns.Duties;

public sealed class DutyDefOf
{
    static public readonly DutyDef Digger = new DutyDef("Digger", PlannerDefOf.Digging).SetTool(ToolUseDefOf.Digging);
    static public readonly DutyDef Miner = new DutyDef("Miner").SetTool(ToolUseDefOf.Mining);
    static public readonly DutyDef Hauler = new("Hauler", PlannerDefOf.Refueling, PlannerDefOf.Hauling, PlannerDefOf.Restocking, PlannerDefOf.Withdraw);
    static public readonly DutyDef Lumberjack = new DutyDef("Lumberjack", PlannerDefOf.Chopping).SetTool(ToolUseDefOf.Chopping);
    static public readonly DutyDef Forester = new("Forester");
    static public readonly DutyDef Craftsman = new DutyDef("Craftsman", PlannerDefOf.Crafting).SetTool(ToolUseDefOf.Building);
    static public readonly DutyDef Smelter = new("Smelter");
    static public readonly DutyDef Farmer = new DutyDef("Farmer", PlannerDefOf.Tilling, PlannerDefOf.Sowing, PlannerDefOf.Harvesting).SetTool(ToolUseDefOf.Argiculture);
    static public readonly DutyDef Harvester = new("Harvester");
    static public readonly DutyDef Forager = new("Forager", PlannerDefOf.Foraging);
    static public readonly DutyDef Builder = new DutyDef("Builder", PlannerDefOf.Deconstructing, PlannerDefOf.Building).SetTool(ToolUseDefOf.Building);
    static public readonly DutyDef Carpenter = new DutyDef("Carpenter").SetTool(ToolUseDefOf.Carpentry);
    static public readonly DutyDef Cook = new("Cook");
    static public readonly DutyDef Scribe = new("Scribe");
    static public readonly DutyDef Aclhemist = new("Aclhemist");
    static public readonly DutyDef Guide = new("Guide");
    static public readonly DutyDef MiscDuties = new("MiscDuties", PlannerDefOf.Switching);
    static public readonly DutyDef Cashier = new("Cashier", PlannerDefOf.Sell);
    static public readonly DutyDef Innkeeper = new("Innkeeper", PlannerDefOf.LodgingRegister);
    static public readonly DutyDef Healer = new("Healer", PlannerDefOf.OfferHealing);
    static public readonly DutyDef Repairsmith = new("Repairsmith", ServiceRepairsDefOf.PlannerVendor);
    static DutyDefOf()
    {
        Def.Register(typeof(DutyDefOf));
    }
}