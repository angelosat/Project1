using Project1.Core.Needs;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Effects
{
    internal class SleepEffectWorker : EntityEffectWorker
    {
        public override EffectDef Def => throw new System.NotImplementedException();

        protected override void OnStart(Actor actor, EntityEffectWrapper wrapper)
        {
            actor.GetNeed(NeedDefOf.Energy).AddMod(EffectDefOf.Sleeping, Ticks.FromMinutes(10));
        }
        protected override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
        {
            actor.GetNeed(NeedDefOf.Energy).RemoveMod(EffectDefOf.Sleeping);
        }
    }
}