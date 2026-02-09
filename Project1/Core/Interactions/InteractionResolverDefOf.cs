using Project1.Framework;

namespace Project1.Core.Interactions
{
    [EnsureStaticCtorCall]
    static class InteractionResolverDefOf
    {
        public static readonly InteractionResolverDef WorkSpeed = new("WorkSpeed", typeof(WorkSpeedResolver));
        static InteractionResolverDefOf()
        {
            Def.Register(typeof(InteractionResolverDefOf));
        }
    }
}
