using System;

#nullable enable

namespace Project1.Framework.Helpers
{
    class Scheduler
    {
        ulong NextTick;
        readonly int RangeMin;
        readonly int RangeMax;
        readonly Random? Random;

        public Scheduler(int rangemin, int rangemax, Random rand)
        {
            this.Random = rand;
            this.RangeMin = rangemin;
            this.RangeMax = rangemax;
        }
        public Scheduler(int delay)
        {
            this.RangeMin = delay;
        }
        private void Reschedule(ulong currentTick)
           => this.NextTick = currentTick + (ulong)(this.Random is Random rand ? rand.Next(this.RangeMin, this.RangeMax) : this.RangeMin);
        public bool OnSchedule(ulong currentTick)
        {
            if (currentTick >= this.NextTick)
            {
                this.Reschedule(currentTick);
                return true;
            }
            return false;
        }
    }
}
