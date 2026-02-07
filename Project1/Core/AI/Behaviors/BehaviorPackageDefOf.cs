using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.Base;

namespace Project1.Core.AI.Behaviors
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
