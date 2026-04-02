using Project1.Framework;

namespace Project1.Core.Effects
{
    [EnsureStaticCtorCall]
    static class EffectDefOf
    {
        static public EffectDef Sleeping = new("Sleeping", new SleepEffectWorker());
        static public EffectDef ModifyNeed = new("ModifyNeed", new EffectModifyNeedWorker());
        static EffectDefOf()
        {
            Def.Register(typeof(EffectDefOf));
        }
    }
}
