using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
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
            //info.AddTabAction("Shop", () => { });

            //var shop = map.Town.ShopManager.GetShops().FirstOrDefault(s => s.Counter.HasValue && s.Counter.Value == vector3);
            //info.AddInfo(new Label($"Shop: {shop?.Name ?? "none"}"));
        }
    }
}

