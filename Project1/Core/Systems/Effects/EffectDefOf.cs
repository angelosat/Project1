using Project1.Core.Needs;
using Project1.Core.Resources;
using Project1.Core.Systems.Magic;
using Project1.Framework;

namespace Project1.Core.Systems.Effects;

[EnsureStaticCtorCall]
static class EffectDefOf
{
    static public EffectDef ModifyNeed = new("ModifyNeed", "Modify", new EffectModifyNeedWorker(), typeof(NeedDef), baseMagnitude: 10);
    static public EffectDef FortifyResource = new("FortifyResource", "Fortify", new Effect_FortifyResource(), typeof(ResourceDef), baseDuration: Ticks.FromDays(1), baseMagnitude: 50) { School = SpellSchoolDefOf.Holy };
    static public EffectDef RestoreResource = new("RestoreResource", "Restore", new Effect_RestoreResource(), typeof(ResourceDef), baseMagnitude: 50) { School = SpellSchoolDefOf.Holy };
    static public EffectDef TownTeleport = new("TownPortal", "Restore", new Effect_TownTeleport(), null);
    static EffectDefOf()
    {
        Def.Register(typeof(EffectDefOf));
    }
}
