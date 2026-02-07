using Project1.Core.Legacy.Storage;
using Project1.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Core.Towns.Zones
{
    internal class StorageFiltersManager
    {
        public delegate Zone ZoneCreator(ZoneManager manager);
        static public ZoneCreator StockpileCreator = new(m => new Stockpile(m));
        static public ZoneCreator ShoppingAreaCreator = new(m => new Stockpile(m));
        static public ZoneCreator DumpCreator = new(m => new Stockpile(m));

        static public void All(Stockpile sp)
        {
            sp.Settings.SetAllowed(ItemCategoryDefOf.RawMaterials, true);
            sp.Settings.SetAllowed(ItemCategoryDefOf.FoodCooked, true);
            sp.Settings.SetAllowed(ItemCategoryDefOf.FoodRaw, true);
            sp.Settings.SetAllowed(ItemCategoryDefOf.Manufactured, true);
        }

        static public void None(Stockpile sp)
        {
            foreach (var f in sp.Settings.Allowed)
                f.Value.Enabled = false;
        }
    }
}
