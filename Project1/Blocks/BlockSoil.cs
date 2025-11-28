using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;

namespace Start_a_Town_
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
            //this.LootTable = new LootTable(
            //            new Loot(() => ItemFactory.CreateFrom(RawMaterialDef.Bags, MaterialDefOf.Soil), 1f, 1, RawMaterialDef.Bags.StackCapacity)
            //            );
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

        public override void RandomBlockUpdate(INetPeer net, IntVec3 global, Cell celll)
        {
            if (net.Map.GetBlock(global + IntVec3.UnitZ) != BlockDefOf.Air)
                return;
            if (net.Map.GetSunLight(global + IntVec3.UnitZ) < 8)
                return;

            // make grass grow anywhere, not just spread from existing grass
            Block.Place(BlockDefOf.Grass, net.Map, global, celll.Material, 0, celll.Variation, 0);

            foreach (var n in global.GetNeighborsDiag())
            {
                if (!net.Map.TryGetCell(n, out Cell cell))
                    continue;
                if (cell.Block != BlockDefOf.Grass)
                    continue;
                Block.Place(BlockDefOf.Grass, net.Map, global, cell.Material, 0, celll.Variation, 0);
                return;
            }
        }
    }
}
