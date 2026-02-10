using Project1.Core.Needs;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Effects
{
    internal class ModifyNeedEffectWorker : EntityEffectWorker
    {
        public override void OnStart(Actor actor, EntityEffectWrapper wrapper)
        {
            var need = actor.GetNeed((NeedDef)wrapper.Target);
            if (wrapper.IsInstant)
                need.ApplyDelta(wrapper.Budget);
            else
                need.AddMod(EffectDefOf.ModifyNeed, wrapper.Rate);
        }
        public override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
        {
            if (!wrapper.IsInstant)
                actor.GetNeed((NeedDef)wrapper.Target).RemoveMod(EffectDefOf.ModifyNeed);
        }
    }
}
