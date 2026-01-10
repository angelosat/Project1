using System.Collections.Generic;

namespace Start_a_Town_
{
    public record struct BlocksUpdatedEvent(MapBase Map, IEnumerable<IntVec3> Positions) : IEventPayload { }
}
