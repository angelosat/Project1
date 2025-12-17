namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static public class WorkstationDefOf
    {
        static public readonly WorkstationDef Smeltery = new("Smeltery", [MaterialMappingDefOf.Ingots]);
        static WorkstationDefOf()
        {
            Def.Register(typeof(WorkstationDefOf));
        }
    }
}
