using Microsoft.Xna.Framework;
using Project1.Core.Construction;
using Project1.Core.Graphics.Particles;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Systems.Materials;

namespace Project1.Core.Blocks
{
    class BlockSoil : Block
    {
        public override bool IsMinable => true;
        public override Color DirtColor => Color.SaddleBrown;
           
        public override ParticleEmitterSphere GetEmitter()
        {
            return base.GetDirtEmitter();
        }

        public BlockSoil()
            : base("Soil")
        {
            this.BreakProduct = RawMaterialDefOf.Bags;
            this.RequiresConstruction = false;
            this.LoadVariations("soil/soil1", "soil/soil2", "soil/soil3", "soil/soil4");
            this.Ingredient = 
                new Ingredient()
                .SetAllow(RawMaterialDefOf.Bags, true)
                .SetAllow(MaterialDefOf.Soil, true);
            this.BuildProperties.Category = ConstructionCategoryDefOf.Structural;
            this.DefaultMaterial = MaterialDefOf.Soil;
            this.DrawMaterialColor = false;
        }
    }
}
