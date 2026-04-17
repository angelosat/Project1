using Project1.Core.AI;
using Project1.Core.Systems.Alchemy;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Tools;
using Project1.Core.Towns.Duties;
using Project1.Framework;

namespace Project1.Core.Crafting
{
    [EnsureStaticCtorCall]
    internal static class WorkstationCapabilityDefOf
    {
        static public readonly WorkstationCapabilityDef Smelting = new("Smelting", typeof(WorkstationCapabilitySmelting), DutyDefOf.Smelter) 
        {
            Output = typeof(MaterialRefinementDef), 
            OutputSpecific = [MaterialRefinementDefOf.Ingots],
            Plan = PlanDefOf.Crafting,
        };
        static public readonly WorkstationCapabilityDef Carpentry = new("Carpentry", typeof(WorkstationCapabilityCarpentryWorker), DutyDefOf.Carpenter)
        {
            Output = typeof(MaterialRefinementDef),
            OutputSpecific = [MaterialRefinementDefOf.Planks],
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef ToolMaking = new("ToolMaking", typeof(WorkstationCapabilityToolMaking), DutyDefOf.Craftsman)
        {
            Output = typeof(ToolProfileDef),
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef Repairing = new("Repairing", typeof(WorkstationCapabilityRepairing), DutyDefOf.Craftsman)
        {
            Plan = PlanDefOf.Repairing
        };
        static public readonly WorkstationCapabilityDef Cooking = new("Cooking", typeof(WorkstationCapabilityCooking), DutyDefOf.Cook)
        {
            Output = typeof(ConsumableDef),
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef Scribing = new("Scribing", typeof(WorkstationCapabilityScribing), DutyDefOf.Scribe)
        {
            Output = typeof(ConsumableDef),
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef Alchemy = new("Alchemy", typeof(WorkstationCapabilityAlchemy), DutyDefOf.Alchemist)
        {
            Output = typeof(ConsumableDef),
            Plan = PlanDefOf.Crafting
        };
        static WorkstationCapabilityDefOf()
        {
            Def.Register(typeof(WorkstationCapabilityDefOf));
        }
    }
}
