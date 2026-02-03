using Project1.Framework.Base;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static public class WorkstationDefOf
    {
        static public readonly WorkstationDef Smeltery = new("Smeltery", capabilities: [WorkstationCapabilityDefOf.Smelting]);
        static public readonly WorkstationDef Workbench = new("Workbench", capabilities: [WorkstationCapabilityDefOf.ToolMaking, WorkstationCapabilityDefOf.Repairing], maxModules: 3);
        static public readonly WorkstationDef Kitchen = new("Kitchen", capabilities: [WorkstationCapabilityDefOf.Cooking]);
        static WorkstationDefOf()
        {
            Def.Register(typeof(WorkstationDefOf));
        }
    }
}
