using System;

namespace Start_a_Town_
{
    internal class ItemBias(Entity entity, int value)
    {
        public readonly int EntityID = entity.RefId;
        public int Value = value;
        TickAccumulatorWorker Worker = new();

        public int Tick()
        {
            //int delta = Worker.GetUpdatesSinceLastTick();
            //if (this.Value > 0)
            //  this.Value = Math.Max(this.Value - delta, 0);
            //else if (this.Value < 0)
            //  this.Value = Math.Min(this.Value + delta, 0);

            if (this.Value == 0)
                return 0;

            int delta = Worker.GetUpdatesSinceLastTick();

            if (Math.Abs(delta) >= Math.Abs(this.Value))
                this.Value = 0;
            else
                this.Value -= Math.Sign(this.Value) * delta;
            return this.Value;
        }
    }
}
