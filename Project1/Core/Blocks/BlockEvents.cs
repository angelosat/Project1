using Project1.Core.Entities.Actors;
using Project1.Core.Networking.Simulation;
using Project1.Core.Simulation;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using Project1.Framework.Events;
using System.Collections.Generic;

namespace Project1.Core.Blocks
{
    public record struct BlockEntityCompUpdatedEvent(BlockComp Comp) : IEventPayload { }
    public record struct BlockEntityUpdatedEvent(BlockEntity Entity) : IEventPayload { }
    public record struct BlockOwnerChangedEvent(BlockEntity Entity, Actor Owner, EntityRefId PreviousOwner) : IEventPayload { }
    public record struct PlayerChangedBlockOwnerEvent(BlockEntity Entity, Actor Actor) : IEventPayload { }
    public record struct BlocksChangedEvent(MapBase Map, IEnumerable<SetBlockArgs> Changes) : IEventPayload { }
    public record struct BlockEntityRemovedEvent(BlockEntity Entity) : IEventPayload { }
    public record struct BlockEntityAddedEvent(BlockEntity Entity) : IEventPayload { }
    public record struct BlockSetEvent(SetBlockArgs Args) : IEventPayload { }
    public record struct PlayerPaintedBlockEvent(IntVec3 Global, Block Block, MaterialDef Material, byte State, int Variation, int Orientation) : IEventPayload { }
    public record struct BlockHitPointsDepletedEvent(IntVec3 Cell) : IEventPayload { }
    public record struct BlockDamagedEvent(MapBase Map, IntVec3 Cell, int Delta) : IEventPayload { }

}