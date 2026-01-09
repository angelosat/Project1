namespace Start_a_Town_
{
    internal class SleepEffectWorker : EntityEffectWorker
    {
        public override void OnStart(Actor actor)
        {
            actor.GetNeed(NeedDefOf.Energy).AddMod(EffectDefOf.Sleeping, Ticks.FromMinutes(1));// 3));
        }
        public override void OnFinish(Actor actor)
        {
            actor.GetNeed(NeedDefOf.Energy).RemoveMod(EffectDefOf.Sleeping);
        }
    }
}
