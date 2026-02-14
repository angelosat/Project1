using Project1.Framework.Events;
using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Materials;
using Project1.Core.Towns.Stockpiles;

namespace Project1.Core.Crafting
{
    internal record struct CraftOrderAddedEvent(BlockWorkstationComp Comp, CraftingOrder Order) : IEventPayload { }
    internal record struct CraftOrderRemovedEvent(BlockWorkstationComp Comp, CraftingOrder Order) : IEventPayload { }
    internal record struct CraftOrderUpdatedEvent(CraftingOrder Order) : IEventPayload { }
    internal record struct CraftOrderReorderedEvent(CraftingOrder Order) : IEventPayload { }
    internal record struct WorkstationUpdatedEvent(BlockWorkstationComp Comp) : IEventPayload { }
    internal record struct CraftOrderCompletedEvent(CraftingOrder Order, Actor Actor) : IEventPayload { }
    internal record struct PlayerSetWorkstationZoneEvent(BlockWorkstationComp Workstation, WorkstationIOType IOType, Stockpile Stockpile) : IEventPayload { }
    internal record struct PlayerModifiedOrderFiltersEvent(CraftingOrder Order, BoneDef Bone, MaterialRefinementDef Refinement, MaterialDef Material) : IEventPayload { }
    internal record struct PlayerModifiedStockpileFiltersEvent(Stockpile Stockpile, ItemDef Item, Def Profile, MaterialDef Material) : IEventPayload { }
    internal record struct PlayerIssuedCraftOrderEvent(BlockWorkstationComp Workstation, Def Craftable) : IEventPayload { }
    internal record struct PlayerIssuedCraftOrderEventNew(BlockWorkstationComp Workstation, AddOrderRequest request) : IEventPayload { }
}
