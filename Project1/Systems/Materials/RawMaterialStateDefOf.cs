namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static internal class RawMaterialStateDefOf
    {
        //static public readonly RawMaterialStateDef Raw = new("Raw");
        //static public readonly RawMaterialStateDef Refined = new("Refined");
        //static public readonly RawMaterialStateDef Processed = new("Processed");
        //static public readonly RawMaterialStateDef Ground = new("Ground");
        static public readonly RawMaterialStateDef Ore = new("Ore", MaterialTypeDefOf.Metal, ItemContent.OreGrayscale);
        static public readonly RawMaterialStateDef Ingots = new("Ingots", MaterialTypeDefOf.Metal, ItemContent.BarsGrayscale);

        static public readonly RawMaterialStateDef Logs = new("Logs", MaterialTypeDefOf.Wood, ItemContent.LogsGrayscale);
        static public readonly RawMaterialStateDef Planks = new("Planks", MaterialTypeDefOf.Wood, ItemContent.PlanksGrayscale);

        static public readonly RawMaterialStateDef Chunk = new("Chunk", MaterialTypeDefOf.Stone, ItemContent.OreGrayscale);

        static RawMaterialStateDefOf()
        {
            Def.Register(typeof(RawMaterialStateDefOf));
        }
    }
}
