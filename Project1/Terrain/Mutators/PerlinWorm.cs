using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Terraforming.Mutators
{
    class PerlinWormGenerator : Terraformer
    {
        readonly TerraformerProperty CaveCount = new("Cave frequency", 5, 0, 20, 1);
        readonly TerraformerProperty CaveSteps = new("Cave average length", 64, 0, 128, 1); //32

        public override IEnumerable<TerraformerProperty> GetAdjustableParameters()
        {
            yield return this.CaveCount;
            yield return this.CaveSteps;
        }
        public override void Generate(MapBase map)
        {
            var r = map.World.Random;
            for (int i = 0; i < map.ActiveChunks.Count; i++)
            {
                for (int j = 0; j < this.CaveCount.Value; j++)
                {
                    var x = r.Next(map.GetSizeInChunks() * Chunk.Size);
                    var y = r.Next(map.GetSizeInChunks() * Chunk.Size);
                    var z = r.Next(map.GetHeightmapValue(x, y));
                    var worm = new PerlinWorm(map, new Vector3(x, y, z), (int)this.CaveSteps.Value, map.World.Seed);
                    foreach (var segment in worm.GetBoxes())
                        this.Carve(map, segment);
                }
            }
        }
      
        void Carve(MapBase map, BoundingBox bbox)
        {
            foreach (var current in bbox.GetBoxIntVec3())
            {
                if (current.Z == 0)
                    continue;

                var c = map.GetCell(current);
                if (c is null)
                    continue;
                if (c.Block == BlockDefOf.Air ||
                    c.Block == BlockDefOf.Fluid
                    //||
                    //c.Block == BlockDefOf.Sand
                    )
                    continue;
                c.Block = BlockDefOf.Air;
            }
        }
      
        class PerlinWorm
        {
            private readonly int MaxLength;

            static readonly double pipi = Math.PI * Math.PI;
            Vector3 Origin;
            readonly int Seed;
            readonly MapBase Map;
            public PerlinWorm(MapBase map, Vector3 origin, int maxLength, int seed)
            {
                this.Map = map;
                this.MaxLength = maxLength;
                this.Origin = origin;
                this.Seed = seed;
            }

            Vector3 GetNextDirectionSpherical(Vector3 global, int seed, float radius)
            {
                return this.GetNextDirectionSpherical((int)global.X, (int)global.Y, (int)global.Z, seed, radius);
            }
           
            Vector3 GetNextDirectionSpherical(int x, int y, int z, int seed, float radius)
            {
                var f = 16;
                
                var s = BitConverter.GetBytes(seed);
                var s2 = BitConverter.GetBytes(seed * seed);

                var rtheta = Generator.Perlin3D(x, y, z, f, s);
                var rphi = Generator.Perlin3D(x, y, z, f, s2);
                
                var atheta = (Math.PI / 2) * (1 + rtheta);
                var aphi = pipi * rphi;

                var rx = (Math.Sin(atheta) * Math.Cos(aphi));
                var ry = (Math.Sin(atheta) * Math.Sin(aphi));
                var rz = Math.Cos(atheta);

                var dir = new Vector3((float)rx, (float)ry, (float)rz);
               
                dir *= radius;
                return dir;
            }
            float GetRadius(float minRadius, float maxRadius, float life, IntVec3 pos)
            {
                maxRadius = 1;
                minRadius = 1;
                var perc = 1 - Math.Abs(life * 2 - 1);
                var radius = minRadius + perc * (maxRadius - minRadius);

                var z = pos.Z;
                var a = 1 - z / (float)MapBase.MaxHeight;
                radius *= a * a * 3;

                return radius;
            }
            
            public IEnumerable<BoundingBox> GetBoxes()
            {
                /// deeper caves should be bigger
                var maxlife = this.MaxLength;
                var current = this.Origin;
                var r = GetRadius(1, 2, 0, current);

                yield return GetBox(current, r);
                for (int i = 0; i < maxlife; i++)
                {
                    current += GetNextDirectionSpherical(current, this.Seed, r);
                    if (!this.Map.Contains(current))
                        continue;
                    r = GetRadius(1, 2, i / (float)maxlife, current);
                    var box = GetBox(current, r);
                    yield return box;
                }
            }
            BoundingBox GetBox(Vector3 global, float radius)
            {
                int x = (int)Math.Round(global.X - radius);
                int y = (int)Math.Round(global.Y - radius);
                int z = (int)Math.Round(global.Z - radius);
                int xx = (int)Math.Round(global.X + radius);
                int yy = (int)Math.Round(global.Y + radius);
                int zz = (int)Math.Round(global.Z + radius);
                return new BoundingBox(new Vector3(x, y, z), new Vector3(xx, yy, zz));
            }
        }
    }
}
