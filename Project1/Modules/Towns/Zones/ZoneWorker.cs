using Project1.Framework.Base;
using Project1.Framework.WorldGen;

namespace Start_a_Town_
{
    public abstract class ZoneWorker
    {
        public abstract bool IsValidLocation(MapBase map, IntVec3 global);
    }
}
