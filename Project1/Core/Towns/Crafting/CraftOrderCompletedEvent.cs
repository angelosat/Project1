using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Materials;
using Project1.Core.Entities;
using Project1.Core.Animations;

namespace Project1.Core.Towns.Crafting
{
    internal record struct CraftOrderCompletedEvent(OrderSettings Order, Actor Actor) : IEventPayload { }
    internal record struct PlayerSetWorkstationZoneEvent(BlockWorkstationComp Workstation, WorkstationIOType IOType, Stockpile Stockpile) : IEventPayload { }
    internal record struct PlayerModifiedOrderFiltersEvent(OrderSettings Order, BoneDef Bone, MaterialRefinementDef Refinement, MaterialDef Material) : IEventPayload { }
    internal record struct PlayerModifiedStockpileFiltersEvent(Stockpile Stockpile, ItemDef Item, Def Profile, MaterialDef Material) : IEventPayload { }
}
