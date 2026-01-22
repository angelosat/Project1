namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static internal class MaterialRefinementDefOf
    {
        static public readonly MaterialRefinementDef Ore = new("Ore", null, MaterialTypeDefOf.Metal, ItemContent.OreGrayscale);
        static public readonly MaterialRefinementDef Ingots = new("Ingots", Ore, MaterialTypeDefOf.Metal, ItemContent.BarsGrayscale) { FuelConsumption = 10 };

        static public readonly MaterialRefinementDef Logs = new("Logs", null, MaterialTypeDefOf.Wood, ItemContent.LogsGrayscale) { FuelProduction = 20 };
        static public readonly MaterialRefinementDef Planks = new("Planks", Planks, MaterialTypeDefOf.Wood, ItemContent.PlanksGrayscale);

        static public readonly MaterialRefinementDef Chunk = new("Chunk", null, MaterialTypeDefOf.Stone, ItemContent.OreGrayscale);

        static MaterialRefinementDefOf()
        {
            Def.Register(typeof(MaterialRefinementDefOf));
        }
    }
}
