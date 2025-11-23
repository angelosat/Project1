using System;
using Start_a_Town_.Net;

namespace Start_a_Town_.Terraforming.Mutators
{
    class TerraformerGrass : Terraformer
    {
        Random Randomizer;

        public TerraformerGrass()
        {
            this.Finalize = (RandomThreaded random, WorldBase w, Cell c, int x, int y, int z) =>
            {

            };
        }

        public override Terraformer SetWorld(WorldBase w)
        {
            this.Randomizer = new Random(w.Seed + "Grass".GetHashCode());
            return this;
        }

        protected override void Finally(Chunk chunk)
        {
            int varCount = BlockDefOf.Grass.Variations.Count;
            var count = Chunk.Size;
            for (int lx = 0; lx < count; lx++)
            {
                for (int ly = 0; ly < count; ly++)
                {
                    int z = chunk.HeightMap[lx][ly];
                    var c = chunk.GetLocalCell(lx, ly, z);
                    if (c.Block != BlockDefOf.Soil)
                        continue;
                    var variation = (byte)this.Randomizer.Next(varCount);
                    c.Block = BlockDefOf.Grass;
                    c.Material = MaterialDefOf.Soil;
                    c.Variation = variation;
                }
            }
        }
    }
}
