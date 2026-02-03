using Project1.Framework.Base;
using Start_a_Town_;

namespace Project1.Framework.Resources
{
    [EnsureStaticCtorCall]
    public static class ResourceDefOf
    {
        static public readonly ResourceDef Health = new("Health", typeof(Health));
        static public readonly ResourceDef Stamina = new("Stamina", typeof(Stamina));
        static public readonly ResourceDef Durability = new("Durability", typeof(Durability));
        static public readonly ResourceDef HitPoints = new("Hit Points", typeof(HitPoints));

        static ResourceDefOf()
        {
            Def.Register(typeof(ResourceDefOf));
        }
    }
}
