namespace Start_a_Town_
{
    internal class ModifyNeedEffectWorker : EntityEffectWorker
    {
        public override void OnStart(Actor actor, EntityEffectWrapper wrapper)
        {
            actor.GetNeed((NeedDef)wrapper.Target).AddMod(EffectDefOf.ModifyNeed, Ticks.FromMinutes(1));
        }
        public override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
        {
            actor.GetNeed((NeedDef)wrapper.Target).RemoveMod(EffectDefOf.ModifyNeed);
        }
    }
}
