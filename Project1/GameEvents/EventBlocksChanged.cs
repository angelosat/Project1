using System.Collections.Generic;

namespace Start_a_Town_.GameEvents
{
    class EventBlocksChanged
    {
        static public object[] Write(MapBase map, IEnumerable<IntVec3> positions)
        {
            return new object[] { map, positions };
        }
        static public void Read(object[] p, out MapBase map, out IEnumerable<IntVec3> positions)
        {
            map = p[0] as MapBase;
            positions = (IEnumerable<IntVec3>)p[1];
        }
    }
}
