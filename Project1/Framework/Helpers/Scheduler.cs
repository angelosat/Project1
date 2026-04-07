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
        readonly Action? Callback;
        public Scheduler(ulong currentTick, int rangemin, int rangemax, Random rand) : this(rangemin, rangemax, rand)
        {
            this.ScheduleFrom(currentTick);
        }
        public Scheduler(Action callback, ulong currentTick, int rangemin, int rangemax, Random rand) : this(currentTick, rangemin, rangemax, rand)
        {
            this.Callback = callback;
        }
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
        public void Tick(ulong currentTick)
        {
            if (currentTick < this.NextTick)
                return;
            this.ScheduleFrom(currentTick);
            this.Callback?.Invoke();
        }
        private Scheduler ScheduleFrom(ulong currentTick)
        {
            this.NextTick = currentTick + (ulong)(this.Random is Random rand ? rand.Next(this.RangeMin, this.RangeMax) : this.RangeMin);
            return this;
        }
        public bool OnSchedule(ulong currentTick)
        {
            if (currentTick >= this.NextTick)
            {
                this.ScheduleFrom(currentTick);
                return true;
            }
            return false;
        }
    }
}
