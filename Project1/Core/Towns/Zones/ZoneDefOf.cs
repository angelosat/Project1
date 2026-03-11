using Project1.Core.Systems.Plants;
using Project1.Core.Towns.Stockpiles;
using Project1.Framework;

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
