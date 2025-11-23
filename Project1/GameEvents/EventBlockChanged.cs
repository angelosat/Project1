using Microsoft.Xna.Framework;

namespace Start_a_Town_.GameEvents
{
    class EventBlockChanged
    {
        static public object[] Write(MapBase map, IntVec3 global)
        {
            return new object[] { map, global };
        }
        static public void Read(object[] p, out MapBase map, out IntVec3 global)
        {
            map = p[0] as MapBase;
            global = (IntVec3)p[1];
        }
    }
}
