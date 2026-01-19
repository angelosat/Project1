namespace Start_a_Town_
{
    internal class AdventuringEffectWorker : EntityEffectWorker
    {
        public override void OnStart(Actor actor, EntityEffectWrapper wrapper)
        {
            var rate = wrapper.Rate; //Ticks.FromMinutes(30);
            var need = actor.GetNeed(AdventurerNeedsDefOf.Adventuring);
            if (rate == 0)
                need.Value += wrapper.Budget;// 100;
            else
                need.AddMod(EffectDefOf.Adventuring, rate); //Ticks.FromSeconds(2));
        }
        public override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
        {
            var rate = wrapper.Rate; //Ticks.FromMinutes(30);
            actor.GetNeed(AdventurerNeedsDefOf.Adventuring).RemoveMod(EffectDefOf.Adventuring);
        }
    }
}
