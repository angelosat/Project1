using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Materials;
using Project1.Core.Towns.Constructions.Categories;
using Project1.Core.Blocks;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core
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
        public override BlockEntity GetBlockEntityOrNew(MapBase map, IntVec3 originGlobal, BlockEntityComp.Spec args)
        {
            return new BlockChestEntity(this.BlockDef, originGlobal, 16);
        }
    }
}
