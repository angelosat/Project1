using Project1.Core.Towns.AI.Needs;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Effects
{
    internal class AdventuringEffectWorker : EntityEffectWorker
    {
        public override EffectDef Def => null;

        protected override void OnStart(Actor actor, EntityEffectWrapper wrapper)
        {
            //var rate = wrapper.Rate;
            //var need = actor.GetNeed(AdventurerNeedsDefOf.Adventuring);
            //if (rate == 0)
            //    need.Value += wrapper.Budget;
            //else
            //    need.AddMod(EffectDefOf.Adventuring, rate); 
        }
        protected override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
        {
            //var rate = wrapper.Rate;
            //actor.GetNeed(AdventurerNeedsDefOf.Adventuring).RemoveMod(EffectDefOf.Adventuring);
        }
    }
}
