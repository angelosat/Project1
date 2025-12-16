namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public static class MaterialMappingDefOf
    {
        static public readonly MaterialMappingDef Log = new("Log", MaterialTypeDefOf.Wood, MaterialProcessDefOf.Raw);
        static public readonly MaterialMappingDef Sticks = new("Sticks", MaterialTypeDefOf.Wood, MaterialProcessDefOf.Processed);
        static public readonly MaterialMappingDef Planks = new("Planks", MaterialTypeDefOf.Wood, MaterialProcessDefOf.Refined);
        static public readonly MaterialMappingDef Sawdust = new("Sawdust", MaterialTypeDefOf.Wood, MaterialProcessDefOf.Ground);

        static public readonly MaterialMappingDef Ore = new("Ore", MaterialTypeDefOf.Metal, MaterialProcessDefOf.Raw);
        static public readonly MaterialMappingDef Ingots = new("Ingots", MaterialTypeDefOf.Metal, MaterialProcessDefOf.Processed);
        static public readonly MaterialMappingDef Plates = new("Plates", MaterialTypeDefOf.Metal, MaterialProcessDefOf.Refined);
        static public readonly MaterialMappingDef Powder = new("Powder", MaterialTypeDefOf.Metal, MaterialProcessDefOf.Ground);

        static public readonly MaterialMappingDef Meat = new("Meat", MaterialTypeDefOf.Flesh, MaterialProcessDefOf.Raw);
        static public readonly MaterialMappingDef Scrapings = new("Scrapings", MaterialTypeDefOf.Flesh, MaterialProcessDefOf.Processed);
        static public readonly MaterialMappingDef Steaks = new("Steaks", MaterialTypeDefOf.Flesh, MaterialProcessDefOf.Refined);
        static public readonly MaterialMappingDef Paste = new("Paste", MaterialTypeDefOf.Flesh, MaterialProcessDefOf.Ground);

        static public readonly MaterialMappingDef Rock = new("Rock", MaterialTypeDefOf.Stone, MaterialProcessDefOf.Raw);
        static public readonly MaterialMappingDef Cobbles = new("Cobbles", MaterialTypeDefOf.Stone, MaterialProcessDefOf.Processed);
        static public readonly MaterialMappingDef Slab = new("Slab", MaterialTypeDefOf.Stone, MaterialProcessDefOf.Refined);
        static public readonly MaterialMappingDef Gravel = new("Gravel", MaterialTypeDefOf.Stone, MaterialProcessDefOf.Ground);

        static MaterialMappingDefOf() => Def.Register(typeof(MaterialMappingDefOf));
        
    }
}
