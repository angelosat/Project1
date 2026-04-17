using Project1.Core.Needs;
using Project1.Core.Resources;
using Project1.Core.Systems.Magic;
using Project1.Framework;

namespace Project1.Core.Effects;

[EnsureStaticCtorCall]
static class EffectDefOf
{
    static public EffectDef Sleeping = new("Sleeping", "<undefined>", new SleepEffectWorker(), null);
    static public EffectDef ModifyNeed = new("ModifyNeed", "Modify", new EffectModifyNeedWorker(), typeof(NeedDef));
    static public EffectDef FortifyResource = new("FortifyResource", "Fortify", new Effect_FortifyResource(), typeof(ResourceDef));
    static public EffectDef RestoreResource = new("RestoreResource", "Restore", new Effect_RestoreResource(), typeof(ResourceDef));
    static EffectDefOf()
    {
        Def.Register(typeof(EffectDefOf));
    }
}
