using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Minerals : Terraformer
    {
        class Template
        {
            public string Name;
            public int Frequency;
            public MaterialDef Material;
            public int MaxVeinSize;
            public int MinZ, MaxZ;

            public Template(string name, MaterialDef material, int frequency, int maxVeinSize, int minZ, int maxZ)
            {
                this.Name = name;
                this.Frequency = frequency;
                this.Material = material;
                this.MaxVeinSize = maxVeinSize;
                this.MinZ = minZ;
                this.MaxZ = maxZ;
            }

            public Template Clone()
            {
                return new Template(this.Name, this.Material, this.Frequency, this.MaxVeinSize, this.MinZ, this.MaxZ);
            }
            public Template SetWorld(WorldBase w)
            {
                return this;
            }
        }

        static readonly Template Coal = new("Coal", MaterialDefOf.Coal, 64, 40, 1, 128);
        static readonly Template Iron = new("Iron", MaterialDefOf.Iron, 32, 30, 1, 96);
        static readonly Template Gold = new("Gold", MaterialDefOf.Gold, 16, 20, 1, 64);
        static readonly Dictionary<string, Template> Dictionary = new Dictionary<string, Template>(){
            {Coal.Name, Coal},
            {Iron.Name, Iron},
            {Gold.Name, Gold}
        };
        readonly List<Template> Templates = new();
        public override Terraformer SetWorld(WorldBase w)
        {
            foreach (var item in Dictionary)
                this.Templates.Add(item.Value.Clone().SetWorld(w));
            return this;
        }
     
        public override void Generate(MapBase map)
        {
            var mapSize = map.GetSizeInChunks() * Chunk.Size;
            foreach (var ore in this.Templates)
            {
                var seed = ore.Name.GetHashCode() + map.World.Seed;
                Random r = new Random(seed);
                var maxVeins = ore.Frequency * map.ActiveChunks.Count;
                for (int i = 0; i < maxVeins; i++)
                {
                    int x = r.Next(mapSize);
                    int y = r.Next(mapSize);
                    int z = r.Next(ore.MaxZ);
                    var next = new Vector3(x, y, z);

                    var c = map.GetCell(next);
                    if (c.Block != BlockDefOf.Cobblestone)
                        continue;
                    c.Block = BlockDefOf.Mineral;
                    c.Material = ore.Material;

                    for (int j = 0; j < ore.MaxVeinSize; j++)
                    {

                        next += VectorHelper.Adjacent[r.Next(6)]; //slow?

                        if (!map.IsInBounds(next))
                            continue;
                        c = map.GetCell(next);
                        if (c.Block != BlockDefOf.Cobblestone)
                            continue;

                        c.Block = BlockDefOf.Mineral;
                        c.Material = ore.Material;
                    }
                }
            }
        }

    }
}
