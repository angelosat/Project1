using Project1.Framework.Effects;
using Project1.Framework.Needs;
using Start_a_Town_;

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
