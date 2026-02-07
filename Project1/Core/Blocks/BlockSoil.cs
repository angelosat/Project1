using Microsoft.Xna.Framework;
using Project1.Core.Materials;
using Project1.Core.Towns.Constructions.Categories;
using Project1.Core.Base;
using Project1.Core.Blocks;
using Project1.Core.Graphics.Particles;
using Project1.Core.Helpers;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Net;
using Project1.Core.Simulation;

namespace Project1.Core
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
            this.BuildProperties.Category = ConstructionCategoryDefOf.Walls;
            this.DefaultMaterial = MaterialDefOf.Soil;
            this.DrawMaterialColor = false;
        }

        public override void RandomBlockUpdate(INetEndpoint net, IntVec3 global, Cell celll)
        {
            if (net.Map.GetBlock(global + IntVec3.UnitZ) != BlockDefOf.Air.Worker)
                return;
            if (net.Map.GetSunLight(global + IntVec3.UnitZ) < 8)
                return;

            // make grass grow anywhere, not just spread from existing grass
            Block.Place(BlockDefOf.Grass.Worker, net.Map, global, celll.Material, 0, celll.Variation, 0);

            foreach (var n in global.GetNeighborsDiag())
            {
                if (!net.Map.TryGetCell(n, out Cell cell))
                    continue;
                if (cell.Block != BlockDefOf.Grass.Worker)
                    continue;
                Block.Place(BlockDefOf.Grass.Worker, net.Map, global, cell.Material, 0, celll.Variation, 0);
                return;
            }
        }
    }
}
