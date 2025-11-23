using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Start_a_Town_.Terraforming
{
    class GradientLowRes
    {
        public WorldBase World;
        readonly int StartX, StartY;
        static readonly int Hash = "gradient".GetHashCode();
        readonly byte[] Seed;
        readonly int Resolution = 8;
        readonly int Turbulence = 1;
        public GradientLowRes(WorldBase world, Chunk chunk, int turb = 1)
        {
            this.World = world;
            this.StartX = chunk.Start.X;
            this.StartY = chunk.Start.Y;
            this.Seed = BitConverter.GetBytes(this.World.Seed + Hash);
            this.Turbulence = turb;
        }
        public GradientLowRes(WorldBase world, int startX, int startY, int turb = 1)
        {
            this.World = world;
            this.StartX = startX;
            this.StartY = startY;
            this.Seed = BitConverter.GetBytes(this.World.Seed + Hash);
            this.Turbulence = turb;
        }

        readonly Dictionary<Vector3, double> noiseCache = new();
        double SampleNoiseAt(int i, int j, int k)
        {
            var x = this.StartX + i;
            var y = this.StartY + j;
            var z = k;
            var xyz = new Vector3(x, y, z);
            var vec = new Vector3(i, j, k);
            if (this.noiseCache.TryGetValue(vec, out double cached))
                return cached;

            var sample = this.GenerateGradient(x, y, z);
            this.timessampled++;
            this.noiseCache[vec] = sample;
            return sample;
        }
        public int timessampled = 0;
        public double GetGradient(int i, int j, int k)
        {
            var res = 8;
            var x1 = res * (i / res);
            var x2 = x1 + res;
            var y1 = res * (j / res);
            var y2 = y1 + res;
            var z1 = res * (k / res);
            var z2 = z1 + res;
            var dx = (i % this.Resolution) / (float)this.Resolution;
            var dy = (j % this.Resolution) / (float)this.Resolution;
            var dz = (k % this.Resolution) / (float)this.Resolution;

            var noise = this.linearInterpolate3d(
                this.SampleNoiseAt(x1, y1, z1), this.SampleNoiseAt(x2, y1, z1),
                this.SampleNoiseAt(x1, y2, z1), this.SampleNoiseAt(x2, y2, z1),
                this.SampleNoiseAt(x1, y1, z2), this.SampleNoiseAt(x2, y1, z2),
                this.SampleNoiseAt(x1, y2, z2), this.SampleNoiseAt(x2, y2, z2),
                dx, dy, dz
                );

            return noise;
        }

        private double linearInterpolate3d(double xm_ym_zm, double xp_ym_zm, double xm_yp_zm, double xp_yp_zm,
                                            double xm_ym_zp, double xp_ym_zp, double xm_yp_zp, double xp_yp_zp,
                                            double x, double y, double z)
        {
            return (xm_ym_zm * (1 - x) * (1 - y) * (1 - z)) + (xp_ym_zm * x * (1 - y) * (1 - z)) + (xm_yp_zm * (1 - x) * y * (1 - z)) + (xp_yp_zm * x * y * (1 - z)) +
                    (xm_ym_zp * (1 - x) * (1 - y) * z) + (xp_ym_zp * x * (1 - y) * z) + (xm_yp_zp * (1 - x) * y * z) + (xp_yp_zp * x * y * z);
        }

        public double GetGradientSlow(int x, int y, int z)
        {
            double grad = 0;
            int octaves = 8;
            for (int k = 0; k < octaves; k++)
            {
                double kk = Math.Pow(2, k);
                grad += Generator.Perlin3D(x, y, z, 0x100 >> k, this.Seed) / kk;
            }
            return grad / octaves;
        }
        public double GenerateGradient(int x, int y, int z)
        {
            double grad = 0;
            int octaves = 8;// 6;
            for (int k = 0; k < octaves; k++)
            {
                double kk = Math.Pow(2, octaves - k);
                var f = 1 << k;
                grad += this.Turbulence * Generator.Perlin3D(x, y, z, f, this.Seed) / kk;

            }
            return grad / octaves;
        }
    }
}
