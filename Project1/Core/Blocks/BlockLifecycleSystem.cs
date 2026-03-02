using Project1.Core.Simulation;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Blocks
{
    internal class BlockLifecycleSystem : SimulationSystem
    {
        readonly Queue<IntVec3> ToRemove = [];

        public BlockLifecycleSystem(MapBase map) : base(map)
        {
            map.Events.ListenTo<BlockHitPointsDepletedEvent>(OnBlockHitpointsDepleted);
        }

        private void OnBlockHitpointsDepleted(BlockHitPointsDepletedEvent e)
        {
            if (this.Map.Net.IsClient)
                return;
            this.ToRemove.Enqueue(e.Cell);
        }

        public override void Tick()
        {
            while (this.ToRemove.Count > 0)
            {
                var cell = this.ToRemove.Dequeue();
                WorldMutations.BreakBlock(this.Map, cell);
            }
        }
    }
}
