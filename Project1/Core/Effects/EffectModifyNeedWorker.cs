using Project1.Core.Entities.Actors;
using Project1.Core.Needs;

namespace Project1.Core.Effects
{
    internal class EffectModifyNeedWorker : EntityEffectWorker
    {
        protected override void OnStart(Actor actor, EntityEffectWrapper wrapper)
        {
        }
        protected override void OnTick(Actor actor, EntityEffectWrapper wrapper)
        {
            var need = actor.Needs.NeedsNew[(NeedDef)wrapper.Target];
            //need.Accumulator += 1f / wrapper.TicksPerUnit;
            need.ApplyAccumulatorDelta(1f / wrapper.TicksPerUnit);
        }
        protected override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
        {
        }
    }
}
