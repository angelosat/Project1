using System.Linq;

namespace Start_a_Town_
{
    //public abstract class TickAccumulatorWrapper : MetricWrapper
    //{
    //    public float TicksPerNaturalDecay = 1 / Ticks.FromSeconds(10);
    //    public float Accumulator;
    //    public int Value;
    //    public override void Tick()
    //    {
    //    }
    //}
    public class TickAccumulatorWorker
    {
        public float TicksPerNaturalDecay = 1 / Ticks.FromSeconds(10);
        public float Accumulator;
        public int GetUpdatesSinceLastTick()
        {
            this.Accumulator -= this.TicksPerNaturalDecay;

            int whole = (int)this.Accumulator;
            if (whole != 0)
            {
                this.Accumulator -= whole;
                return whole;
            }
            return 0;
        }
        public void SetSecondsPerDecay(int seconds)
        {
            this.TicksPerNaturalDecay = 1 / Ticks.FromSeconds(seconds);
        }
    }
}
