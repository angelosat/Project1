using Project1.Core.Base;
using Project1.Core.Net.Packets;
using Project1.Core.Simulation;
using System.Collections.Generic;
namespace Project1.Core.WorldGen
{
    public record struct CellsInvalidatedEvent(MapBase Map, IEnumerable<IntVec3> Positions) : IEventPayload { }
    public record struct BlocksChangedEvent(MapBase Map, IEnumerable<SetBlockArgs> Changes) : IEventPayload { }
}
