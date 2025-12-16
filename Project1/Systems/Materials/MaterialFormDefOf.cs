namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static public class MaterialFormDefOf
    {
        static public readonly MaterialFormDef Raw = new("Raw", RawMaterialDefOfNew.Raw);
        static public readonly MaterialFormDef Refined = new("Refined", RawMaterialDefOfNew.Refined);
        static public readonly MaterialFormDef Processed = new("Processed", RawMaterialDefOfNew.Processed);
        static public readonly MaterialFormDef Ground = new("Ground", RawMaterialDefOfNew.Ground);
        static MaterialFormDefOf() => Def.Register(typeof(MaterialFormDefOf));
    }
}
