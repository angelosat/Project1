using Project1.Core.AI;
using Project1.Core.Materials;
using Project1.Core.Tools;
using Project1.Framework;

namespace Project1.Core.Crafting
{
    [EnsureStaticCtorCall]
    internal static class WorkstationCapabilityDefOf
    {
        static public readonly WorkstationCapabilityDef Smelting = new("Smelting", typeof(WorkstationCapabilitySmeltingWorker)) 
        {
            ProfileCategory = typeof(MaterialRefinementDef), 
            SpecificRecipes = [MaterialRefinementDefOf.Ingots],
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef Carpentry = new("Carpentry", typeof(WorkstationCapabilityCarpentryWorker))
        {
            ProfileCategory = typeof(MaterialRefinementDef),
            SpecificRecipes = [MaterialRefinementDefOf.Planks],
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef ToolMaking = new("ToolMaking", typeof(WorkstationCapabilityToolMakingWorker))
        {
            ProfileCategory = typeof(ToolProfileDef),
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef Repairing = new("Repairing", typeof(WorkstationCapabilityRepairingWorker))
        {
            Plan = PlanDefOf.Repairing
        };
        static public readonly WorkstationCapabilityDef Cooking = new("Cooking", typeof(WorkstationCapabilityRepairingWorker))
        {
            Plan = null
        };
        static WorkstationCapabilityDefOf()
        {
            Def.Register(typeof(WorkstationCapabilityDefOf));
        }
    }
}
