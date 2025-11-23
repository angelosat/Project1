using Start_a_Town_.Terraforming.Mutators;

namespace Start_a_Town_
{
    public static class TerraformerDefOf
    {
        public static readonly TerraformerDef Sea = new("Sea", typeof(TerraformerSea));
        public static readonly TerraformerDef Land = new("Land", typeof(Land));
        public static readonly TerraformerDef Normal = new("Normal", typeof(TerraformerNormal));
        public static readonly TerraformerDef Grass = new("Grass", typeof(TerraformerGrass));
        public static readonly TerraformerDef Flowers = new("Flowers", typeof(TerraformerFlowers));
        public static readonly TerraformerDef Trees = new("Trees", typeof(GeneratorPlants));
        public static readonly TerraformerDef Caves = new("Caves", typeof(Caves));
        public static readonly TerraformerDef Minerals = new("Minerals", typeof(Minerals));
        public static readonly TerraformerDef Empty = new("Empty", typeof(Empty));
        public static readonly TerraformerDef PerlinWorms = new("PerlinWorms", typeof(PerlinWormGenerator));
        static TerraformerDefOf()
        {
            Def.Register(typeof(TerraformerDefOf));
        }
    }
}
