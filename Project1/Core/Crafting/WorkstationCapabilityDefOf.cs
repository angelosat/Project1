using Project1.Core.AI;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Tools;
using Project1.Framework;

namespace Project1.Core.Crafting
{
    [EnsureStaticCtorCall]
    internal static class WorkstationCapabilityDefOf
    {
        static public readonly WorkstationCapabilityDef Smelting = new("Smelting", typeof(WorkstationCapabilitySmeltingWorker)) 
        {
            Output = typeof(MaterialRefinementDef), 
            OutputSpecific = [MaterialRefinementDefOf.Ingots],
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef Carpentry = new("Carpentry", typeof(WorkstationCapabilityCarpentryWorker))
        {
            Output = typeof(MaterialRefinementDef),
            OutputSpecific = [MaterialRefinementDefOf.Planks],
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef ToolMaking = new("ToolMaking", typeof(WorkstationCapabilityToolMakingWorker))
        {
            Output = typeof(ToolProfileDef),
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef Repairing = new("Repairing", typeof(WorkstationCapabilityRepairingWorker))
        {
            Plan = PlanDefOf.Repairing
        };
        static public readonly WorkstationCapabilityDef Cooking = new("Cooking", typeof(WorkstationCapabilityCookingWorker))
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
