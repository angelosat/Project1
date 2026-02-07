using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Core.Materials;
using Project1.Core.Towns.Constructions.Categories;
using Project1.Core.Blocks;
using Project1.Core.Graphics.Particles;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Materials;

namespace Project1.Core
{
    class BlockSand : Block
    {
        public override bool IsMinable => true;
        public BlockSand()
            : base("Sand")
        {
            this.LoadVariations("sand1");
            this.Ingredient = new Ingredient()
                .SetAllow(MaterialDefOf.Sand, true);
            this.BuildProperties.Category = ConstructionCategoryDefOf.Walls;
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            yield return MaterialDefOf.Sand;
        }
        public override ParticleEmitterSphere GetEmitter()
        {
            var e = base.GetDirtEmitter();

            e.ColorBegin = e.ColorEnd = Color.Gold;
            return e;
        }
    }
}
