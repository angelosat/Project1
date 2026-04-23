using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Legacy.Storage;
using Project1.Core.Systems.Materials;
using Project1.Core.Towns.Stockpiles;
using Project1.Framework.Events;
using Project1.Framework.Serialization;
using System.Runtime.CompilerServices;

namespace Project1.Core.Systems.Crafting;

internal record struct CraftOrderAddedEvent(BlockWorkstationComp Comp, CraftingOrder Order) : IEventPayload;
internal record struct CraftOrderRemovedEvent(BlockWorkstationComp Comp, CraftingOrder Order) : IEventPayload;
internal record struct CraftOrderUpdatedEvent(CraftingOrder Order) : IEventPayload;
internal record struct CraftOrderReorderedEvent(CraftingOrder Order) : IEventPayload;
internal record struct WorkstationUpdatedEvent(BlockWorkstationComp Comp) : IEventPayload;
internal record struct CraftOrderCompletedEvent(CraftingOrder Order, Actor Actor) : IEventPayload;
internal record struct PlayerSetWorkstationZoneEvent(BlockWorkstationComp Workstation, WorkstationIOType IOType, Stockpile Stockpile) : IEventPayload;
internal record struct PlayerModifiedOrderFiltersEvent(CraftingOrder Order, BoneDef Bone, MaterialTypeDef Refinement, MaterialDef Material) : IEventPayload;
internal record struct PlayerModifiedStockpileFiltersEvent(Stockpile Stockpile, ItemDef Item, Def Profile, MaterialDef Material) : IEventPayload;
internal record struct PlayerModifiedStockpileSettingsEvent(Stockpile Stockpile, bool ForSale, StoragePriority Priority) : IEventPayload;
internal record struct PlayerIssuedCraftOrderEventNew(BlockWorkstationComp Workstation, AddOrderRequest Request) : IEventPayload;
internal record struct PlayerCancellingUnfinishedItemEvent(Entity Item) : IEventPayload;
internal record struct ActorFinishedCraftingEvent(EntityRefId Actor, CraftingOrderId Order, EntityRefId Product) : IEventPayload, ISerializableNewNew<ActorFinishedCraftingEvent>
{
    public static ActorFinishedCraftingEvent Create(IDataReader r)
        => new(r.ReadId<EntityRefId>(), r.ReadId<CraftingOrderId>(), r.ReadId<EntityRefId>());
    
    public readonly IDataWriter Write(IDataWriter w)
    {
        w.Write(this.Actor);
        w.Write(this.Order);
        w.Write(this.Product);
        return w;
    }
}
