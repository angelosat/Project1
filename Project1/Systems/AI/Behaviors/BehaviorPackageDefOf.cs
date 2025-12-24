using Start_a_Town_.AI.Behaviors;

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
