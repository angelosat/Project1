namespace Start_a_Town_
{
    public class RawMaterialStageDef(string name) : Def(name)
    {
    }
    [EnsureStaticCtorCall]
    static internal class RawMaterialStageDefOf
    {
        static public readonly RawMaterialStageDef Raw = new("Raw");
        static public readonly RawMaterialStageDef Refined = new("Refined");
        static public readonly RawMaterialStageDef Processed = new("Processed");
        static public readonly RawMaterialStageDef Ground = new("Ground");
        static RawMaterialStageDefOf()
        {
            Def.Register(typeof(RawMaterialStageDefOf));
        }
    }
}
