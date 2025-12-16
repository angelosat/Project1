namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static public class MaterialProcessDefOf
    {
        static public readonly MaterialProcessDef Raw = new("Raw");
        static public readonly MaterialProcessDef Refined = new("Refined");
        static public readonly MaterialProcessDef Processed = new("Processed");
        static public readonly MaterialProcessDef Ground = new("Ground");
        static MaterialProcessDefOf() => Def.Register(typeof(MaterialProcessDefOf));
    }
}
