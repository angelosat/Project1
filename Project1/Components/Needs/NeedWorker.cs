using System.Linq;

namespace Start_a_Town_
{
    public abstract class NeedWorker : MetricWorker
    {
        public sealed override void Tick(MetricWrapper wrapper)
        {
            var need = (Need)wrapper;

            need.Accumulator += need.Mods.Sum(m => m.RateMod);
            need.Accumulator -= need.TicksPerNaturalDecay;

            int whole = (int)need.Accumulator;
            if (whole != 0)
            {
                need.Accumulator -= whole;
                need._valueInt += whole;
            }

            this.TickExtra(need);
        }

        protected virtual void TickExtra(Need need) { }
       
    }
}
