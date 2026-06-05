using Project1.Core.AI;
using Project1.Core.Systems.Alchemy;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Consumables.Scrolls;
using Project1.Core.Systems.Cooking;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Tools;
using Project1.Core.Towns.Duties;
using Project1.Framework;

namespace Project1.Core.Systems.Crafting
{
    [EnsureStaticCtorCall]
    internal static class WorkstationCapabilityDefOf
    {
        static public readonly WorkstationCapabilityDef Smelting = new("Smelting", typeof(WorkstationCapability_Smelting), DutyDefOf.Smelter) 
        {
            OrderType = typeof(AddOrderRequest),
            Output = typeof(MaterialRefinementDef), 
            OutputSpecific = [MaterialRefinementDefOf.Ingots],
            Plan = PlanDefOf.Crafting,
        };
        static public readonly WorkstationCapabilityDef Carpentry = new("Carpentry", typeof(WorkstationCapability_Carpentry), DutyDefOf.Carpenter)
        {
            OrderType = typeof(AddOrderRequest),
            Output = typeof(MaterialRefinementDef),
            OutputSpecific = [MaterialRefinementDefOf.Planks],
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef ToolMaking = new("ToolMaking", typeof(WorkstationCapability_ToolMaking), DutyDefOf.Craftsman)
        {
            OrderType = typeof(AddOrderRequest),
            Output = typeof(GearProfileDef),
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef Repairing = new("Repairing", typeof(WorkstationCapability_Repairing), DutyDefOf.Craftsman)
        {
            OrderType = typeof(AddOrderRequest),
            Plan = PlanDefOf.Repairing
        };
        static public readonly WorkstationCapabilityDef Cooking = new("Cooking", typeof(WorkstationCapabilityCooking), DutyDefOf.Cook)
        {
            OrderType = typeof(AddOrderRequest),
            Output = typeof(ConsumableDef),
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef Scribing = new("Scribing", typeof(WorkstationCapability_Scribing), DutyDefOf.Scribe)
        {
            OrderType = typeof(AddOrderRequest_Scribing),
            Output = typeof(ConsumableDef),
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef Alchemy = new("Alchemy", typeof(WorkstationCapability_Alchemy), DutyDefOf.Alchemist)
        {
            OrderType = typeof(AddOrderRequest_Alchemy),
            //OrderType = typeof(AddOrderRequest),
            Output = typeof(ConsumableDef),
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef PlantProcessing = new("PlantProcessing", typeof(WorkstationCapability_PlantProcessing), DutyDefOf.Cook)
        {
            OrderType = typeof(AddOrderRequest_ExtractSeeds),
            Output = typeof(ConsumableDef),
            Plan = PlanDefOf.Crafting
        };
        static WorkstationCapabilityDefOf()
        {
            Def.Register(typeof(WorkstationCapabilityDefOf));
        }
    }
}
