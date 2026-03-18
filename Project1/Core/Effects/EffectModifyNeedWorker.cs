using Project1.Core.Entities.Actors;
using Project1.Core.Needs;

namespace Project1.Core.Effects
{
    internal class EffectModifyNeedWorker : EntityEffectWorker
    {
        public override void OnStart(Actor actor, EntityEffectWrapper wrapper)
        {
            //var need = actor.GetNeed((NeedDef)wrapper.Target);
            //if (wrapper.IsInstant)
            //    need.ApplyDelta(wrapper.Budget.Value);
            //else
            //    need.AddMod(EffectDefOf.ModifyNeed, wrapper.Rate);
        }
        public override void Tick(Actor actor, EntityEffectWrapper wrapper)
        {
            //if (wrapper.RemainingBudget == 0)
            //    wrapper.IsFinished = true;
            //actor.GetNeed((NeedDef)wrapper.Target).RemoveMod(EffectDefOf.ModifyNeed);
            var need = actor.Needs.NeedsNew[(NeedDef)wrapper.Target];
            need.Accumulator += 1f / wrapper.TicksPerUnit;
        }
        public override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
        {
            //if (!wrapper.IsInstant)
                //actor.GetNeed((NeedDef)wrapper.Target).RemoveMod(EffectDefOf.ModifyNeed);
        }
    }
}
