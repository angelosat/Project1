using System.Linq;
using Project1.Core.Blocks;
using Project1.Core.Base;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Graphics;
using Project1.Core.Rooms;
using Project1.Core.Simulation;
using Project1.Core.UI.Hud;
using Project1.Framework.Math;

namespace Project1.Core.Towns.Shops.Blocks
{
    class BlockShopCounter : Block, IBlockWorkstation
    {
        AtlasDepthNormals.Node.Token[] Orientations = TexturesCounter;

        public BlockShopCounter() 
            : base("ShopCounter", 0, 1, false, true)
        {
            this.HidingAdjacent = false;
            this.Variations.Add(this.Orientations.First());
            this.Furniture = FurnitureDefOf.Counter;
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
        }
       
        protected override void GetQuickButtonsEx(SelectionManager info, MapBase map, IntVec3 vector3)
        {
           
        }
    }
}