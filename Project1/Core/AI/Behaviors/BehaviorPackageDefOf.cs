using Project1.Framework;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.AI.Behaviors
{
    [EnsureStaticCtorCall]
    internal static class BehaviorPackageDefOf
    {
        static public readonly BehaviorPackageDef Npc = new("Npc", new BehaviorQueue(
                                                                       new BehaviorMemory(),
                                                                       new BehaviorHandleResources(),
                                                                       new BehaviorHandleOrders(),
                                                                       new BehaviorHandlePlans()));
        static BehaviorPackageDefOf()
        {
            Def.Register(typeof(BehaviorPackageDefOf));
        }
    }
}
