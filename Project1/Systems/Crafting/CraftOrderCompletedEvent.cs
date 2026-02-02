using Project1.Framework.Animations;

namespace Start_a_Town_
{
    internal record struct CraftOrderCompletedEvent(OrderSettings Order, Actor Actor) : IEventPayload { }
    internal record struct StockpileUpdatedEvent(Stockpile Stockpile) : IEventPayload { }
    internal record struct PlayerSetWorkstationZoneEvent(BlockWorkstationComp Workstation, WorkstationIOType IOType, Stockpile Stockpile) : IEventPayload { }
    internal record struct PlayerModifiedOrderFiltersEvent(OrderSettings Order, BoneDef Bone, MaterialRefinementDef Refinement, MaterialDef Material) : IEventPayload { }
    internal record struct PlayerModifiedStockpileFiltersEvent(Stockpile Stockpile, ItemDef Item, Def Profile, MaterialDef Material) : IEventPayload { }
}
