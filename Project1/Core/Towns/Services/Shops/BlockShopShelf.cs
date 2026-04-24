using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Blocks;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Rooms;
using Project1.Framework.Helpers;

#nullable enable

namespace Project1.Core.Towns.Services.Shops
{
    class BlockShopShelf : Block//, IBlockWorkstation
    {
        //AtlasDepthNormals.Node.Token[] Orientations = TexturesCounter;

        public BlockShopShelf()
            : base("ShopCounter", 0, 1, false, true)
        {
            //this.HidingAdjacent = false;
            //this.Variations.Add(this.Orientations.First());
            this.HidingAdjacent = false;
            var tex = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/furniture/stool").ToGrayscale();
            this.Variations.Add(Atlas.Load("stoolgrayscale", tex, BlockDepthMap, NormalMap));
            this.Furniture = FurnitureDefOf.Counter;
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
        }
    }
}