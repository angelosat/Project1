using Project1.Framework;
using Project1.Core.Crafting;

namespace Project1.Core
{
    [EnsureStaticCtorCall]
    static public class WorkstationDefOf
    {
        static public readonly WorkstationDef Smeltery = new("Smeltery", capabilities: [WorkstationCapabilityDefOf.Smelting]);
        static public readonly WorkstationDef Carpentry = new("Carpentry", capabilities: [WorkstationCapabilityDefOf.Carpentry]);
        static public readonly WorkstationDef Workbench = new("Workbench", capabilities: [WorkstationCapabilityDefOf.ToolMaking, WorkstationCapabilityDefOf.Repairing], maxModules: 3);
        static public readonly WorkstationDef Kitchen = new("Kitchen", capabilities: [WorkstationCapabilityDefOf.Cooking]);
        static WorkstationDefOf()
        {
            Def.Register(typeof(WorkstationDefOf));
        }
    }
}
