using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    partial class BlockChest : Block
    {
        static Texture2D ChestNormalMap = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/furniture/chestnormal");
        public BlockChest()
            : base("Chest", opaque: false)
        {
            this.HidingAdjacent = false;
            var tex = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/furniture/chest").ToGrayscale();
            this.Variations.Add(Atlas.Load("chestgrayscale", tex, BlockDepthMap, ChestNormalMap));
            this.BuildProperties.Category = ConstructionCategoryDefOf.Furniture;
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            return Def.GetDefs<MaterialDef>().Where(mat => mat.Type == MaterialTypeDefOf.Wood || mat.Type == MaterialTypeDefOf.Metal);
        }
        public override BlockEntity CreateBlockEntity(IntVec3 originGlobal)
        {
            return new BlockChestEntity(originGlobal, 16);
        }
    }
}
