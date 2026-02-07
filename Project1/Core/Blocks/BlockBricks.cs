using Project1.Core.Materials;
using Project1.Core.Towns.Constructions.Categories;
using Project1.Core.Base;
using Project1.Core.Blocks;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Materials;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core
{
    class BlockBricks : Block
    {
        public BlockBricks()
            : base("Bricks")
        {
            this.LoadVariations("bricks/bricks");
            this.BuildProperties.Complexity = 20;
            this.BuildProperties.Category = ConstructionCategoryDefOf.Walls;
            this.Ingredient =
                new Ingredient()
                    .SetAllow(MaterialTypeDefOf.Metal, true)
                    .SetAllow(MaterialTypeDefOf.Stone, true);
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            return Def.GetDefs<MaterialDef>().Where(m => m.Type == MaterialTypeDefOf.Stone || m.Type == MaterialTypeDefOf.Metal);
        }
    }
}
