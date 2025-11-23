using System;
using System.Diagnostics;

namespace Start_a_Town_.Terraforming.Mutators
{
    /// <summary>
    /// TODO: add a height factor to create bigger caves towards bottom of map
    /// </summary>
    class Caves : Terraformer
    {
        static readonly int Hash1 = "ridged1".GetHashCode();
        static readonly int Hash2 = "ridged2".GetHashCode();

        byte[] RidgedSeed;
        byte[] RidgedSeed2;
        int CaveFrequency = 16;//64
        float Threshold = 0.1f;//0.05f
        
        static public Stopwatch Watch = new();

        public override void Initialize(WorldBase w, Cell c, int x, int y, int z, double g)
        {
            Watch.Start();
            if (z == 0)
                return;
            if (c.Block == BlockDefOf.Air ||
                c.Block == BlockDefOf.Fluid ||
                c.Block == BlockDefOf.Sand)
                return;

            double ridged = Generator.Perlin3D(x, y, z, this.CaveFrequency, this.RidgedSeed);

            double ridged2 = Generator.Perlin3D(x, y, z, this.CaveFrequency, this.RidgedSeed2);

            if (ridged > -this.Threshold && ridged < this.Threshold && ridged2 > -this.Threshold && ridged2 < this.Threshold)
                if (z > 0)
                    c.Block = BlockDefOf.Air;
            Watch.Stop();
        }

        private static void CavesVariation(Cell c, int z, double ridged)
        {
            float sealevel = MapBase.MaxHeight / 2f;
            float distanceFromSeaLevel = (sealevel - z) / sealevel;
            if (ridged * distanceFromSeaLevel < -.2f)
                c.Block = BlockDefOf.Air;
        }
        public override Terraformer SetWorld(WorldBase w)
        {
            this.RidgedSeed = BitConverter.GetBytes(w.Seed + Hash1);
            this.RidgedSeed2 = BitConverter.GetBytes(w.Seed + Hash2);
            return this;
        }
    }
}
