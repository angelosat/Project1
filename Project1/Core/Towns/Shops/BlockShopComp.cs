using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using System;


namespace Project1.Core.Towns.Shops
{
    internal class BlockShopComp : BlockComp
    {
        public new class Spec : BlockComp.Spec
        {
            public override Type CompType => typeof(BlockShopComp);

            public override BlockShopComp CreateComp() => new();
        }
        public override BlockCompDef CompDef => BlockCompDefOf.Shop;

        public int CashFloat = 100;


    }
}
