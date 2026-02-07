using Project1.Core.Base;
using Project1.Core.Simulation;

namespace Project1.Core.Towns.Zones
{
    public abstract class ZoneWorker
    {
        public abstract bool IsValidLocation(MapBase map, IntVec3 global);
    }
}
