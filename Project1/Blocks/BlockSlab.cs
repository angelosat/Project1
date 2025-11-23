using System.Collections.Generic;

namespace Start_a_Town_
{
    class BlockSlab : Block
    {
        public BlockSlab()
            : base("Slab", opaque: false)
        {
            this.HidingAdjacent = false;
            var txt = Block.Atlas.Load("blocks/slab", Block.QuarterBlockMapDepth, Block.QuarterBlockMapNormal);
            this.Variations.Add(txt);
            this.Ingredient = new Ingredient(RawMaterialDefOf.Ingots, null, null, 1);
            this.BuildProperties.Category = ConstructionCategoryDefOf.Walls;
        }
        public override float GetPathingCost(byte data)
        {
            return 0;
        }
        public override Microsoft.Xna.Framework.Color[] UV
        {
            get
            {
                return Block.BlockCoordinatesQuarter;
            }
        }
        public override MouseMap MouseMap
        {
            get
            {
                return Block.BlockQuarterMouseMap;
            }
        }
        public override float GetHeight(byte data, float x, float y)
        {
            return this.GetHeight(x, y);
        }
        public override float GetHeight(float x, float y)
        {
            return .25f;
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            yield return MaterialDefOf.Stone;
        }
    }
}
