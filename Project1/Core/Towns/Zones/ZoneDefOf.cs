using Project1.Core.Plants;
using Project1.Core.Base;
using Project1.Core;
using Project1.Core.Helpers;

namespace Project1.Core.Towns.Zones
{
    [EnsureStaticCtorCall]
    static class ZoneDefOf
    {
        public static readonly ZoneDef Stockpile = new("Stockpile", typeof(Stockpile), typeof(ZoneStockpileWorker));
        public static readonly ZoneDef Growing = new("Growing", typeof(GrowingZone), typeof(ZoneGrowingWorker));

        static ZoneDefOf()
        { 
            Def.Register(typeof(ZoneDefOf));
        }
    }
}
