namespace Start_a_Town_
{
    internal class ModifyNeedEffectWorker : EntityEffectWorker
    {
        public override void OnStart(Actor actor, EntityEffectWrapper wrapper)
        {
            var need = actor.GetNeed((NeedDef)wrapper.Target);
            if (wrapper.IsInstant)
                need.ApplyDelta(wrapper.Budget);
            else
                need.AddMod(EffectDefOf.ModifyNeed, wrapper.Rate);// Ticks.FromMinutes(1));
        }
        public override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
        {
            if (!wrapper.IsInstant)
                actor.GetNeed((NeedDef)wrapper.Target).RemoveMod(EffectDefOf.ModifyNeed);
        }
    }
}
