using Project1.Framework;

namespace Project1.Core.Resources
{
    [EnsureStaticCtorCall]
    public static class ResourceDefOf
    {
        static public readonly ResourceDef Health = new("Health", typeof(Health));
        static public readonly ResourceDef Stamina = new("Stamina", typeof(Stamina));
        static public readonly ResourceDef Durability = new("Durability", typeof(Durability));
        static public readonly ResourceDef HitPoints = new("Hit Points", typeof(HitPoints));
        static public readonly ResourceDef Fuel = new("Fuel", typeof(ResourceWorkerPassive));
        static public readonly ResourceDef RepairCharges = new("Repair Charges", typeof(ResourceWorkerPassive));
        static public readonly ResourceDef Assembly = new("Assembly", typeof(ResourceWorkerPassive));

        static ResourceDefOf()
        {
            Def.Register(typeof(ResourceDefOf));
        }
    }
}
