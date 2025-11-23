using System;

namespace Start_a_Town_.Terraforming.Mutators
{
    class TerraformerFlowers : Terraformer
    {
        float Density = 0.3f;
        protected override void Finally(Chunk chunk)
        {
            int zSlice = MapBase.MaxHeight / 2;
            int varCount = BlockGrass.FlowerOverlays.Count;
            byte[][] flowerNoiseSeeds = new byte[varCount][];
            int s = chunk.Map.World.Seed + "flowers".GetHashCode();
            var random = new Random(s);
            for (int i = 0; i < varCount; i++)
                flowerNoiseSeeds[i] = BitConverter.GetBytes(s * i);
            var size = Chunk.Size;
            for (int lx = 0; lx < size; lx++)
            {
                for (int ly = 0; ly < size; ly++)
                {
                    var cell = chunk.GetLocalCell(lx, ly, chunk.HeightMap[lx][ly]);
                    if (cell.Block != BlockDefOf.Grass)
                        continue;

                    int gx = lx + chunk.Start.X, gy = ly + chunk.Start.Y;

                    int oct = 2;
                    double[] n = new double[varCount];
                    double nmax = 0;
                    int selectedFlower = 0;
                    for (int i = 0; i < varCount; i++)
                    {
                        n[i] = 0;
                        var currentSeed = flowerNoiseSeeds[i];
                        for (int o = 0; o < oct; o++)
                            n[i] += Generator.Perlin3D(gx, gy, zSlice, 16 >> o, currentSeed) / Math.Pow(2, o);
                        if (n[i] > nmax) /// select the flower variation that has the highest noise value
                        {
                            nmax = n[i]; //slow?
                            selectedFlower = i;
                        }
                    }

                    nmax += this.Density * 2 - 1;
                    if (nmax > 0)
                    {
                        var r = random.NextDouble();
                        if (r < nmax)
                            cell.BlockData = (byte)(selectedFlower + 1); // because 0 == no flowers
                    }
                }
            }
        }
    }
}
