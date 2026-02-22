using Project1.Core.Construction;
using Project1.Core.Graphics.Particles;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Materials;

namespace Project1.Core.Blocks
{
    class BlockStone : Block
    {
        public override bool IsMinable => true;

        public BlockStone()
            : base("Cobblestone", 0, 1, true, true)
        {
            this.BreakProduct = RawMaterialDefOf.Boulders;
            this.LoadVariations("stone5height19");
            this.Ingredient = new Ingredient()
                .SetAllow(RawMaterialDefOf.Boulders, true)
                .SetAllow(MaterialDefOf.Stone, true);
            this.BuildProperties.Category = ConstructionCategoryDefOf.Structural;
        }
        public override ParticleEmitterSphere GetEmitter()
        {
            return base.GetDustEmitter();
        }
    }
}
