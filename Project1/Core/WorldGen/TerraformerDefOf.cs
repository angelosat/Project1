using Project1.Core.Base;

namespace Project1.Core.WorldGen
{
    public static class TerraformerDefOf
    {
        public static readonly TerraformerDef Sea = new("Sea", typeof(TerraformerSea));
        public static readonly TerraformerDef Land = new("Land", typeof(TerraformerLand));
        public static readonly TerraformerDef Normal = new("Normal", typeof(TerraformerNormal));
        public static readonly TerraformerDef Grass = new("Grass", typeof(TerraformerGrass));
        public static readonly TerraformerDef Flowers = new("Flowers", typeof(TerraformerFlowers));
        public static readonly TerraformerDef Trees = new("Trees", typeof(GeneratorPlants));
        public static readonly TerraformerDef Caves = new("Caves", typeof(TerraformerCaves));
        public static readonly TerraformerDef Minerals = new("Minerals", typeof(TerraformerMinerals));
        public static readonly TerraformerDef Empty = new("Empty", typeof(TerraformerEmpty));
        public static readonly TerraformerDef PerlinWorms = new("PerlinWorms", typeof(PerlinWormGenerator));
        static TerraformerDefOf()
        {
            Def.Register(typeof(TerraformerDefOf));
        }
    }
}
