using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project1.Core.Towns.Shops
{
    internal class BlockShopComp : BlockComp
    {
        public override BlockCompDef CompDef => BlockCompDefOf.Shop;

        HashSet<ZoneId> Stockpiles = [];


    }
}
