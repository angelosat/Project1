namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class FrontierDefOf
    {
        static public readonly FrontierDef Forest = new FrontierDef("Forest", 1)
        {
            LootWeightRawMaterial = 1,
            LootWeightCurrency = 9
        }
        .AddLootRawMaterial(RawMaterialDefOf.Logs,
            (MaterialDefOf.LightWood, 90),
            (MaterialDefOf.DarkWood, 9),
            (MaterialDefOf.RedWood, 1))
        .AddLootRawMaterial(RawMaterialDefOf.Ore,
            (MaterialDefOf.Iron, 95),
            (MaterialDefOf.Gold, 5))
        .AddLootCurrency(1, 20)
        ;

        static public readonly FrontierDef Swamp = new FrontierDef("Swamp", 2)
        {
            LootWeightRawMaterial = 1,
            LootWeightCurrency = 9
        }
        .AddLootRawMaterial(RawMaterialDefOf.Logs,
            (MaterialDefOf.DarkWood, 95),
            (MaterialDefOf.RedWood, 5))
        .AddLootRawMaterial(RawMaterialDefOf.Ore,
            (MaterialDefOf.Iron, 90),
            (MaterialDefOf.Gold, 10))
        .AddLootCurrency(20, 50)
        ;
        static FrontierDefOf()
        {
            Def.Register(Forest);
            Def.Register(Swamp);
        }
    }
}
