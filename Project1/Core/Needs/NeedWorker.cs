using System.Linq;
using Project1.Core.Resources;
using Project1.Core.Helpers;

namespace Project1.Core.Needs
{
    public abstract class NeedWorker : MetricWorker
    {
        public sealed override void Tick(MetricWrapper wrapper)
        {
            var need = (Need)wrapper;

            if (need.Mods.Count > 0)
            {
                var baserate = need.Def.BaseRate;
                //need.Accumulator += need.Mods.Sum(m => m.RateMod) * need.Def.BaseRate;
                foreach (var mod in need.Mods)
                {
                    var consumed = mod.RateMod * baserate;
                    if(mod.TotalBudget.HasValue)
                        mod.TotalBudget -= consumed;
                    need.Accumulator += consumed;
                }
            }
            else
                need.Accumulator -= need.TicksPerNaturalDecay * need.Def.BaseRate;

            int whole = (int)need.Accumulator;
            if (whole != 0)
            {
                need.Accumulator -= whole;
                need.Value += whole;
                this.TickExtra(need);
            }
        }
        protected virtual void TickExtra(Need need) { }
    }
}
