using Project1.Core.Base;
using Project1.Core.Helpers;
using Project1.Core.Materials;
using Project1.Core.Entities;
using Project1.Core.Assets;
using Project1.Core.Tools;

namespace Project1.Core.Plants
{
    [EnsureStaticCtorCall]
    static class PlantSpeciesDefOf
    {
        static public readonly PlantSpeciesDef Berry = new("Berry")
        {
            TextureGrowing = ItemContent.BerryBushGrowing.AssetPath,
            TextureGrown = ItemContent.BerryBushGrown.AssetPath,
            TextureFruit = ItemContent.BerryBushFruit.AssetPath,
            TextureSeeds = ItemContent.SeedsFull.AssetPath,
            SeedsName = "Seeds",
            StemMaterial = MaterialDefOf.ShrubStem,
            FruitMaterial = MaterialDefOf.Berry,
            Growth = new GrowthProperties(ItemDefOf.Fruit, MaterialDefOf.Berry, 5, 6),
            PlantEntity = PlantDefOf.Bush,
        };

        static public readonly PlantSpeciesDef LightTree = new("LightTree")
        {
            TextureGrowing = ItemContent.TreeFull.AssetPath,
            TextureGrown = ItemContent.TreeFull.AssetPath,
            TextureSeeds = ItemContent.Sapling.AssetPath,
            SeedsName = "Saplings",
            StemMaterial = MaterialDefOf.LightWood,
            ProductCutDown = RawMaterialDefOf.Logs,
            MaxYieldCutDown = 5,
            GrowTicks = 6 * Ticks.PerSecond,
            PlantEntity = PlantDefOf.Tree,
            ToolToCut = ToolUseDefOf.Chopping,
            StemHealRate = Ticks.FromHours(1),
            PlantingSpacing = 1
        };
        static PlantSpeciesDefOf()
        {
            Def.Register(typeof(PlantSpeciesDefOf));
        }
    }
}
