using Project1.Core.Simulation;
using System;

namespace Project1.Core.Towns.Terrain
{
    internal abstract class ChunkController(Chunk chunk)
    {
        protected Chunk Chunk = chunk;
        protected MapBase Map => this.Chunk.Map;
        protected Random Random => this.Map.Random;
        ulong NextTick;
        protected abstract int MinTicks { get; }
        protected abstract int MaxTicks { get; }
        private void Reschedule()
            //=> this.NextTick = this.Chunk.Map.World.CurrentTick + (ulong)this.Chunk.Map.Random.Next(Ticks.FromHours(1), Ticks.FromHours(2));
            => this.NextTick = this.Chunk.Map.World.CurrentTick + (ulong)this.Chunk.Map.Random.Next(this.MinTicks, this.MaxTicks);
        bool OnSchedule()
        {
            if (this.Map.World.CurrentTick >= this.NextTick)
            {
                this.Reschedule();
                return true;
            }
            return false;
        }

        internal virtual void ResolveReferences() { }

        internal void Tick()
        {
            if (this.Chunk.Map.Net.IsClient)
                return;
            if (!this.OnSchedule())
                return;
            this.ScheduledTick();
        }
        protected virtual void ScheduledTick() { }
    }
}
