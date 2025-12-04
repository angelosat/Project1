using System.Collections.Generic;

namespace Start_a_Town_
{
    public class BlocksChangedEvent(MapBase map, IEnumerable<IntVec3> positions) : EventPayloadBase
    {
        public readonly MapBase Map = map;
        public readonly IEnumerable<IntVec3> Positions = positions;
    }
}
