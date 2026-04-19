using Microsoft.Xna.Framework;
using Project1.Framework;

namespace Project1.Core.Resources;

[EnsureStaticCtorCall]
public static class ResourceDefOf
{
    static public readonly ResourceDef Health = new("Health", typeof(Health)) { SupportsRestore = true, SupportsFortify = true };
    static public readonly ResourceDef Mana = new("Mana", typeof(ResourceWorkerPassive), baseRegenRate: .01f) { SupportsRestore = true, SupportsFortify = true, Color = Color.RoyalBlue };
    static public readonly ResourceDef Stamina = new("Stamina", typeof(Stamina)) { SupportsRestore = true, SupportsFortify = true };
    static public readonly ResourceDef Durability = new("Durability", typeof(ResourceWorkerPassive));
    static public readonly ResourceDef HitPoints = new("Hit Points", typeof(HitPoints));
    static public readonly ResourceDef Fuel = new("Fuel", typeof(ResourceWorkerPassive));
    static public readonly ResourceDef RepairCharges = new("Repair Charges", typeof(ResourceWorkerPassive));
    static public readonly ResourceDef Assembly = new("Assembly", typeof(ResourceWorkerPassive));
    static public readonly ResourceDef Patience = new("Patience", typeof(ResourceWorker_Patience), baseRegenRate: .001f) { SupportsRestore = true, SupportsFortify = true };
    static public readonly ResourceDef Cash = new("Cash", typeof(ResourceWorkerPassive)) { Color = Color.Yellow };

    static ResourceDefOf()
    {
        Def.Register(typeof(ResourceDefOf));
    }
}
