using Project1.Core.Simulation;
using System;

#nullable enable

namespace Project1.Core.Towns.Terrain
{
    internal abstract class ChunkController(Chunk chunk)
    {
        protected Chunk Chunk = chunk;
        ulong NextTick;

        protected MapBase Map => this.Chunk.Map;
        protected Random Random => this.Map.Random;
        protected abstract int MinTicks { get; }
        protected abstract int MaxTicks { get; }

        private void Reschedule()
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
        
        internal void Tick()
        {
            if (this.Chunk.Map.Net.IsClient)
                return;
            if (!this.OnSchedule())
                return;
            this.ScheduledTick();
        }
        protected virtual void ScheduledTick() { }
        internal virtual void ResolveReferences() { }
    }
}
