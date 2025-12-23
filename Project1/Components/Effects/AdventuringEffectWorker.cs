namespace Start_a_Town_
{
    internal class AdventuringEffectWorker : EntityEffectWorker
    {
        public override void OnStart(Actor actor)
        {
            actor.GetNeed(AdventurerNeedsDefOf.Adventuring).AddMod(EffectDefOf.Adventuring, 0, 1);
        }
        public override void OnFinish(Actor actor)
        {
            actor.GetNeed(AdventurerNeedsDefOf.Adventuring).RemoveMod(EffectDefOf.Adventuring);
        }
    }
}
