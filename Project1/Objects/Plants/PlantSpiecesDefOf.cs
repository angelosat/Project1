using Start_a_Town_.Components;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PlantSpiecesDefOf
    {
        static public readonly PlantProperties Berry = new("Berry")
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

        static public readonly ItemVariantDef BerryNew = new ItemVariantDef(PlantDefOf.Bush, "BerryNew")
            .AddSpec(new PlantComponent.Spec() {  })
            ;

        static public readonly ItemVariantDef SeedsNew = new ItemVariantDef(ItemDefOf.Seeds, "SeedsNew")
        {

        };

        static public readonly PlantProperties LightTree = new("LightTree")
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
        static PlantSpiecesDefOf()
        {
            Def.Register(typeof(PlantSpiecesDefOf));
        }
    }
}
