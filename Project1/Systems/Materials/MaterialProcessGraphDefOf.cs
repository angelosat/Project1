namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static public class MaterialProcessGraphDefOf
    {
        static public readonly MaterialProcessGraphDef Default = new("Default",
            [(MaterialFormDefOf.Raw, [MaterialFormDefOf.Processed, MaterialFormDefOf.Refined, MaterialFormDefOf.Ground]),
            (MaterialFormDefOf.Refined, [MaterialFormDefOf.Processed, MaterialFormDefOf.Ground]),
            (MaterialFormDefOf.Processed, [MaterialFormDefOf.Ground])]
            );

        static MaterialProcessGraphDefOf() => Def.Register(typeof(MaterialProcessGraphDefOf));
    }
}
