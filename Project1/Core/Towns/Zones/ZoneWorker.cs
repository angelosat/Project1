using Project1.Core.Simulation;
using Project1.Framework.Math;

namespace Project1.Core.Towns.Zones
{
    public abstract class ZoneWorker
    {
        public abstract bool IsValidLocation(MapBase map, IntVec3 global);
    }
}
