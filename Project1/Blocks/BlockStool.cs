using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    class BlockStool : Block
    {
        public BlockStool()
            : base("Stool", opaque: false)
        {
            this.HidingAdjacent = false;
            var tex = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/furniture/stool").ToGrayscale();
            this.Variations.Add(Atlas.Load("stoolgrayscale", tex, BlockDepthMap, NormalMap));
            this.Furniture = FurnitureDefOf.Table;
            this.BuildProperties.Category = ConstructionCategoryDefOf.Furniture;
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
            this.UtilitiesProvided.Add(Utility.Types.Eating);
        }

        protected override void Place(MapBase map, IntVec3 global, MaterialDef material, byte data, int variation, int orientation, bool notify = true)
        {
            base.Place(map, global, material, data, variation, orientation, notify);
            map.Town.AddUtility(Utility.Types.Eating, global);
        }
      
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, IntVec3 global, Camera camera, Microsoft.Xna.Framework.Vector4 screenBounds, Microsoft.Xna.Framework.Color sunlight, Microsoft.Xna.Framework.Vector4 blocklight, Microsoft.Xna.Framework.Color fog, Microsoft.Xna.Framework.Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
        {
            DrawShadow(canvas.NonOpaque, global, camera, screenBounds, sunlight, blocklight, fog, tint, depth);
            return base.Draw(canvas, chunk, global, camera, screenBounds, sunlight, blocklight, fog, tint, depth, variation, orientation, data, mat);
        }
    }
}