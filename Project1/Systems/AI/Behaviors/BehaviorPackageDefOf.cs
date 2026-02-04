using Project1.Core.AI.Behaviors;
using Project1.Framework.AI.Behaviors;
using Project1.Framework.Base;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Framework.AI.NodeTypes;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class BehaviorPackageDefOf
    {
        static public readonly BehaviorPackageDef Npc = new("Npc", new BehaviorQueue(
                                                                       new AIMemory(),
                                                                       new BehaviorHandleResources(),
                                                                       new BehaviorHandleOrders(),
                                                                       new BehaviorHandlePlans()));
        static BehaviorPackageDefOf()
        {
            Def.Register(typeof(BehaviorPackageDefOf));
        }
    }
}
