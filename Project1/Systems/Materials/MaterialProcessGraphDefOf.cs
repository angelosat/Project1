namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static public class MaterialProcessGraphDefOf
    {
        static public readonly MaterialProcessGraphDef Default = new(
            [(MaterialProcessDefOf.Raw, [MaterialProcessDefOf.Processed, MaterialProcessDefOf.Refined, MaterialProcessDefOf.Ground]),
            (MaterialProcessDefOf.Refined, [MaterialProcessDefOf.Processed, MaterialProcessDefOf.Ground]),
            (MaterialProcessDefOf.Processed, [MaterialProcessDefOf.Ground])]
            );

        static MaterialProcessGraphDefOf() => Def.Register(typeof(MaterialProcessGraphDefOf));
    }
}
