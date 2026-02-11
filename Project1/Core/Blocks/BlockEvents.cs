using System.Collections.Generic;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Core.Entities.Actors;
using Project1.Core.Materials;
using Project1.Core.Net.Packets;
using Project1.Core.Simulation;

namespace Project1.Core.Blocks
{
    public record struct BlockEntityCompUpdatedEvent(BlockComp Comp) : IEventPayload { }
    public record struct BlockEntityUpdatedEvent(BlockEntity Entity) : IEventPayload { }
    public record struct BlockOwnerChangedEvent(BlockEntity Entity, Actor Actor) : IEventPayload { }
    public record struct PlayerChangedBlockOwnerEvent(BlockEntity Entity, Actor Actor) : IEventPayload { }
    public record struct BlockHitEvent(Block Block, MapBase Map, IntVec3 Global, int Amount) : IEventPayload { }
    public record struct BlockDestroyedEvent(Block Block, MapBase Map, IntVec3 Global) : IEventPayload { }
    public record struct BlocksChangedEvent(MapBase Map, IEnumerable<SetBlockArgs> Changes) : IEventPayload { }
    public record struct BlockEntityRemovedEvent(BlockEntity Entity) : IEventPayload { }
    public record struct BlockEntityAddedEvent(BlockEntity Entity) : IEventPayload { }
    public record struct BlockSetEvent(SetBlockArgs Args) : IEventPayload { }
    internal record struct PlayerPaintedBlockEvent(IntVec3 Global, Block Block, MaterialDef Material, byte State, int Variation, int Orientation) : IEventPayload { }
}