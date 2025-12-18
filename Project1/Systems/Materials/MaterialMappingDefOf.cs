namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public static class MaterialMappingDefOf
    {
        static public readonly MaterialMappingDef Log = new("Log", MaterialTypeDefOf.Wood, RefinementPathDefOf.Raw, ItemContent.LogsGrayscale);
        static public readonly MaterialMappingDef Sticks = new("Sticks", MaterialTypeDefOf.Wood, RefinementPathDefOf.Shaped);
        static public readonly MaterialMappingDef Planks = new("Planks", MaterialTypeDefOf.Wood, RefinementPathDefOf.Cut, ItemContent.PlanksGrayscale);
        static public readonly MaterialMappingDef Sawdust = new("Sawdust", MaterialTypeDefOf.Wood, RefinementPathDefOf.Ground);

        static public readonly MaterialMappingDef Ore = new("Ore", MaterialTypeDefOf.Metal, RefinementPathDefOf.Raw, ItemContent.OreGrayscale);
        static public readonly MaterialMappingDef Ingots = new("Ingots", MaterialTypeDefOf.Metal, RefinementPathDefOf.Shaped, ItemContent.BarsGrayscale);
        static public readonly MaterialMappingDef Plates = new("Plates", MaterialTypeDefOf.Metal, RefinementPathDefOf.Cut);
        static public readonly MaterialMappingDef Powder = new("Powder", MaterialTypeDefOf.Metal, RefinementPathDefOf.Ground);

        static public readonly MaterialMappingDef Meat = new("Meat", MaterialTypeDefOf.Flesh, RefinementPathDefOf.Raw);
        static public readonly MaterialMappingDef Scrapings = new("Scrapings", MaterialTypeDefOf.Flesh, RefinementPathDefOf.Shaped);
        static public readonly MaterialMappingDef Steaks = new("Steaks", MaterialTypeDefOf.Flesh, RefinementPathDefOf.Cut);
        static public readonly MaterialMappingDef Paste = new("Paste", MaterialTypeDefOf.Flesh, RefinementPathDefOf.Ground);

        static public readonly MaterialMappingDef Rock = new("Rock", MaterialTypeDefOf.Stone, RefinementPathDefOf.Raw, ItemContent.OreGrayscale);
        static public readonly MaterialMappingDef Cobbles = new("Cobbles", MaterialTypeDefOf.Stone, RefinementPathDefOf.Shaped);
        static public readonly MaterialMappingDef Slab = new("Slab", MaterialTypeDefOf.Stone, RefinementPathDefOf.Cut);
        static public readonly MaterialMappingDef Gravel = new("Gravel", MaterialTypeDefOf.Stone, RefinementPathDefOf.Ground, ItemContent.BagsGrayscale);

        static MaterialMappingDefOf() => Def.Register(typeof(MaterialMappingDefOf));
        
    }
}
