using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Rooms;
using Project1.Framework.Graphics;
using System.Linq;

namespace Project1.Core.Towns.Services.Shops
{
    class BlockShopShelf : Block//, IBlockWorkstation
    {
        AtlasDepthNormals.Node.Token[] Orientations = TexturesCounter;

        public BlockShopShelf()
            : base("ShopCounter", 0, 1, false, true)
        {
            this.HidingAdjacent = false;
            this.Variations.Add(this.Orientations.First());
            this.Furniture = FurnitureDefOf.Counter;
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
        }
    }

    internal sealed class BlockShelfComp : BlockComp
    {
        public override BlockCompDef CompDef => BlockCompDefOf.Shelf;

        ZoneId InputStockpile = ZoneId.Null;

        internal Entity GetDisplayedItem() => this.Parent.Map.GetEntitiesAt(this.Parent.OriginGlobal.Above).FirstOrDefault();
        internal void SetInput(ZoneId stockpileId)
        {
            this.InputStockpile = stockpileId;
        }
    }
}