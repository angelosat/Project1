namespace Start_a_Town_
{
    internal class AdventuringEffectWorker : EntityEffectWorker
    {
        public override void OnStart(Actor actor, EntityEffectWrapper wrapper)
        {
            actor.GetNeed(AdventurerNeedsDefOf.Adventuring).AddMod(EffectDefOf.Adventuring, Ticks.FromMinutes(30)); //Ticks.FromSeconds(2));
        }
        public override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
        {
            actor.GetNeed(AdventurerNeedsDefOf.Adventuring).RemoveMod(EffectDefOf.Adventuring);
        }
    }
}
