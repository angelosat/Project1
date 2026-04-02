using Project1.Core.Resources;
using Project1.Core.Helpers;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Needs
{
    public abstract class NeedWorker// : MetricWorker
    {
        //public sealed override void Tick(MetricWrapper wrapper)
        public void Tick(Need need)
        {
            //var need = (Need)wrapper;

            if (need.Mods.Count > 0)
            {
                var baserate = need.Def.BaseRate;
                //need.Accumulator += need.Mods.Sum(m => m.RateMod) * need.Def.BaseRate;
                foreach (var mod in need.Mods)
                {
                    var sourceEffect = ((Actor)need.Owner).Effects.GetEffect(mod.EffectDef);
                    var toConsume = mod.RateMod * baserate;
                    //if(mod.TotalBudget.HasValue)
                    //    mod.TotalBudget -= consumed;
                    var consumed = sourceEffect.Consume(toConsume);
                    need.Accumulator += consumed;
                }
            }
            else
                need.Accumulator -= need.TicksPerNaturalDecay * need.Def.BaseRate;

            int whole = (int)need.Accumulator;
            if (whole != 0)
            {
                need.Accumulator -= whole;
                //need.Value += whole;
                need.ApplyDelta(whole);
                this.TickExtra(need);
            }
        }
        protected virtual void TickExtra(Need need) { }
    }
}
