namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static public class MaterialProcessDefOf
    {
        static public readonly MaterialStageDef Raw = new("Raw", RawMaterialDefOfNew.Raw);
        static public readonly MaterialStageDef Refined = new("Refined", RawMaterialDefOfNew.Refined);
        static public readonly MaterialStageDef Processed = new("Processed", RawMaterialDefOfNew.Processed);
        static public readonly MaterialStageDef Ground = new("Ground", RawMaterialDefOfNew.Ground);
        static MaterialProcessDefOf() => Def.Register(typeof(MaterialProcessDefOf));
    }
}
