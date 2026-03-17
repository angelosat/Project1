using Project1.Core.Blocks;
using Project1.Core.Networking.Simulation;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using Project1.Framework.Events;
using System.Collections.Generic;

namespace Project1.Core.Simulation
{
    internal record struct ChunksLoadedEvent : IEventPayload { }
    internal record struct CellsInvalidatedEvent(MapBase Map, IEnumerable<IntVec3> Positions) : IEventPayload { }
    //internal record struct BlocksChangedEvent(MapBase Map, IEnumerable<SetBlockArgs> Changes) : IEventPayload { }
    internal record struct MapEditEvent(MapEditContext Context, MapEditType Type, MapBase Map, HashSet<IntVec3> Targets, Block Block, MaterialDef Material, byte Data, int Variation, int Orientation) : IEventPayload { }
    internal record struct CellEditEvent(CellQuery Edit) : IEventPayload { }
}
