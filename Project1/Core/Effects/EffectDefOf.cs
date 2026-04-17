using Project1.Core.Systems.Magic;
using Project1.Framework;

namespace Project1.Core.Effects;

[EnsureStaticCtorCall]
static class EffectDefOf
{
    static public EffectDef Sleeping = new("Sleeping", "<undefined>", new SleepEffectWorker());
    static public EffectDef ModifyNeed = new("ModifyNeed", "Modify", new EffectModifyNeedWorker());
    static public EffectDef FortifyResource = new("FortifyResource", "Fortify", new Effect_FortifyResource());
    static public EffectDef RestoreResource = new("RestoreResource", "Restore", new Effect_RestoreResource());
    static EffectDefOf()
    {
        Def.Register(typeof(EffectDefOf));
    }
}
