using Project1.Core.Needs;
using Project1.Core.Base;

namespace Project1.Core.Effects
{
    [EnsureStaticCtorCall]
    static class EffectDefOf
    {
        static public EffectDef Sleeping = new("Sleeping", new SleepEffectWorker());
        static public EffectDef Adventuring = new("Adventuring", new AdventuringEffectWorker());
        static public EffectDef ModifyNeed = new("ModifyNeed", new ModifyNeedEffectWorker());
        static EffectDefOf()
        {
            Def.Register(typeof(EffectDefOf));
        }
    }
}
