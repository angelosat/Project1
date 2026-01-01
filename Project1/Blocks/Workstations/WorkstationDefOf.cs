namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static public class WorkstationDefOf
    {
        static public readonly WorkstationDef Smeltery = new("Smeltery", [MaterialRefinementDefOf.Ingots]) { Craftables = [CraftableDefOf.Smelting] };
        static public readonly WorkstationDef Workbench = new("Workbench", [MaterialRefinementDefOf.Ingots], maxModules: 3) { Craftables = [CraftableDefOf.ToolMaking] };
        static public readonly WorkstationDef Kitchen = new("Kitchen", [MaterialRefinementDefOf.Ingots]);
        static WorkstationDefOf()
        {
            Def.Register(typeof(WorkstationDefOf));
        }
    }
}
