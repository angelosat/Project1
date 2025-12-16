namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public static class MaterialMappingDefOf
    {
        static public readonly MaterialMappingDef Log = new("Log", MaterialTypeDefOf.Wood, MaterialFormDefOf.Raw, ItemContent.LogsGrayscale);
        static public readonly MaterialMappingDef Sticks = new("Sticks", MaterialTypeDefOf.Wood, MaterialFormDefOf.Processed);
        static public readonly MaterialMappingDef Planks = new("Planks", MaterialTypeDefOf.Wood, MaterialFormDefOf.Refined, ItemContent.PlanksGrayscale);
        static public readonly MaterialMappingDef Sawdust = new("Sawdust", MaterialTypeDefOf.Wood, MaterialFormDefOf.Ground);

        static public readonly MaterialMappingDef Ore = new("Ore", MaterialTypeDefOf.Metal, MaterialFormDefOf.Raw, ItemContent.OreGrayscale);
        static public readonly MaterialMappingDef Ingots = new("Ingots", MaterialTypeDefOf.Metal, MaterialFormDefOf.Processed, ItemContent.BarsGrayscale);
        static public readonly MaterialMappingDef Plates = new("Plates", MaterialTypeDefOf.Metal, MaterialFormDefOf.Refined);
        static public readonly MaterialMappingDef Powder = new("Powder", MaterialTypeDefOf.Metal, MaterialFormDefOf.Ground);

        static public readonly MaterialMappingDef Meat = new("Meat", MaterialTypeDefOf.Flesh, MaterialFormDefOf.Raw);
        static public readonly MaterialMappingDef Scrapings = new("Scrapings", MaterialTypeDefOf.Flesh, MaterialFormDefOf.Processed);
        static public readonly MaterialMappingDef Steaks = new("Steaks", MaterialTypeDefOf.Flesh, MaterialFormDefOf.Refined);
        static public readonly MaterialMappingDef Paste = new("Paste", MaterialTypeDefOf.Flesh, MaterialFormDefOf.Ground);

        static public readonly MaterialMappingDef Rock = new("Rock", MaterialTypeDefOf.Stone, MaterialFormDefOf.Raw, ItemContent.OreGrayscale);
        static public readonly MaterialMappingDef Cobbles = new("Cobbles", MaterialTypeDefOf.Stone, MaterialFormDefOf.Processed);
        static public readonly MaterialMappingDef Slab = new("Slab", MaterialTypeDefOf.Stone, MaterialFormDefOf.Refined);
        static public readonly MaterialMappingDef Gravel = new("Gravel", MaterialTypeDefOf.Stone, MaterialFormDefOf.Ground, ItemContent.BagsGrayscale);

        static MaterialMappingDefOf() => Def.Register(typeof(MaterialMappingDefOf));
        
    }
}
