using Microsoft.Xna.Framework;
using Project1.Core.Construction;
using Project1.Core.Graphics.Particles;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Materials;
using System.Collections.Generic;

namespace Project1.Core.Blocks
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
