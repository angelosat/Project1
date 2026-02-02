using Project1.Core.Effects;
using Project1.Core.Needs;
using Start_a_Town_;

namespace Project1.Framework.Effects
{
    internal class SleepEffectWorker : EntityEffectWorker
    {
        public override void OnStart(Actor actor, EntityEffectWrapper wrapper)
        {
            actor.GetNeed(NeedDefOf.Energy).AddMod(EffectDefOf.Sleeping, Ticks.FromMinutes(10));// 3));
        }
        public override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
        {
            actor.GetNeed(NeedDefOf.Energy).RemoveMod(EffectDefOf.Sleeping);
        }
    }
}