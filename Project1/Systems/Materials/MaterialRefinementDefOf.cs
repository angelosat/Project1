namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static internal class MaterialRefinementDefOf
    {
        //static public readonly RawMaterialStateDef Raw = new("Raw");
        //static public readonly RawMaterialStateDef Refined = new("Refined");
        //static public readonly RawMaterialStateDef Processed = new("Processed");
        //static public readonly RawMaterialStateDef Ground = new("Ground");
        static public readonly MaterialRefinementDef Ore = new("Ore", null, MaterialTypeDefOf.Metal, ItemContent.OreGrayscale);
        static public readonly MaterialRefinementDef Ingots = new("Ingots", Ore, MaterialTypeDefOf.Metal, ItemContent.BarsGrayscale) { FuelConsumption = 1 };

        static public readonly MaterialRefinementDef Logs = new("Logs", null, MaterialTypeDefOf.Wood, ItemContent.LogsGrayscale) { FuelProduction = 1 };
        static public readonly MaterialRefinementDef Planks = new("Planks", Planks, MaterialTypeDefOf.Wood, ItemContent.PlanksGrayscale);

        static public readonly MaterialRefinementDef Chunk = new("Chunk", null, MaterialTypeDefOf.Stone, ItemContent.OreGrayscale);

        static MaterialRefinementDefOf()
        {
            Def.Register(typeof(MaterialRefinementDefOf));
        }
    }
}
