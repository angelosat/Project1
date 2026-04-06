using Project1.Core.Construction;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Systems.Materials;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Blocks
{
    class BlockBricks : Block
    {
        public BlockBricks()
            : base("Bricks")
        {
            this.LoadVariations("bricks/bricks");
            this.BuildProperties.Complexity = 20;
            this.BuildProperties.Category = ConstructionCategoryDefOf.Structural;
            this.Ingredient =
                new Ingredient()
                    .SetAllow(MaterialTypeDefOf.Metal, true)
                    .SetAllow(MaterialTypeDefOf.Stone, true);
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            return Def.Get<MaterialDef>().Where(m => m.Type == MaterialTypeDefOf.Stone || m.Type == MaterialTypeDefOf.Metal);
        }
    }
}
