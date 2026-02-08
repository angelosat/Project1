using Project1.Core.Simulation;
using Project1.Framework.Math;

namespace Project1.Core.Towns.Zones
{
    class ZoneStockpileWorker : ZoneWorker
    {
        public override bool IsValidLocation(MapBase map, IntVec3 global)
        {
            if (!map.IsSolid(global))
                return false;
            if (map.IsSolid(global.Above))
                return false;
            return true;
        }
    }
}
