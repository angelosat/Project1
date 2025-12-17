namespace Start_a_Town_
{
    public class RawMaterialStateDef(string name) : Def(name)
    {
    }
    [EnsureStaticCtorCall]
    static internal class RawMaterialStageDefOf
    {
        static public readonly RawMaterialStateDef Raw = new("Raw");
        static public readonly RawMaterialStateDef Refined = new("Refined");
        static public readonly RawMaterialStateDef Processed = new("Processed");
        static public readonly RawMaterialStateDef Ground = new("Ground");
        static RawMaterialStageDefOf()
        {
            Def.Register(typeof(RawMaterialStageDefOf));
        }
    }
}
