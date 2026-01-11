using System.Collections.Generic;
namespace Start_a_Town_
{
    //public record struct BlocksUpdatedEvent(MapBase Map, IEnumerable<IntVec3> Positions) : IEventPayload { }
    public record struct CellsInvalidatedEvent(MapBase Map, IEnumerable<IntVec3> Positions) : IEventPayload { }
    public record struct BlocksChangedEvent(MapBase Map, IEnumerable<SetBlockArgs> Changes) : IEventPayload { }
}
