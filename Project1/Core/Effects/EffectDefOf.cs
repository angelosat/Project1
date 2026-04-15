using Project1.Core.Systems.Magic;
using Project1.Framework;

namespace Project1.Core.Effects;

[EnsureStaticCtorCall]
static class EffectDefOf
{
    static public EffectDef Sleeping = new("Sleeping", new SleepEffectWorker());
    static public EffectDef ModifyNeed = new("ModifyNeed", new EffectModifyNeedWorker());
    static public EffectDef FortifyResource = new("FortifyResource", new Effect_FortifyResource());
    static public EffectDef RestoreResource = new("RestoreResource", new Effect_RestoreResource());
    static EffectDefOf()
    {
        Def.Register(typeof(EffectDefOf));
    }
}
