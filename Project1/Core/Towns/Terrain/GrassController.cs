using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Materials;
using Project1.Core.Simulation;

namespace Project1.Core.Towns.Terrain
{
    internal sealed class GrassController(Chunk chunk) : ChunkController(chunk)
    {
        const double TramplingChance = 0.1f;

        protected override int MinTicks => Ticks.FromHours(1);

        protected override int MaxTicks => Ticks.FromHours(2);

        //protected override int MinTicks => 1;

        //protected override int MaxTicks => 2;

        private void HandleActorEnteringNewCell(ActorEnteringNewCellEvent e)
        {
            if (e.Actor.Net.IsClient)
                return;
            var cell = e.Actor.Cell.Below;
            if (!this.Chunk.Contains(cell))
                return;

            var edit = this.Chunk.Edit(cell);
            if (edit.Block != BlockDefOf.Grass)
                return;

            var roll = this.Random.NextSingle();
            if (roll > TramplingChance)
                return;

            edit.Block = BlockDefOf.Soil;
            edit.Flush();
        }

        protected override void ScheduledTick()
        {
            var heightMap = this.Chunk.HeightMap;
            var x = this.Random.Next(Chunk.Size);
            var y = this.Random.Next(Chunk.Size);
            var z = heightMap[x][y];
            var local = new IntVec3Local(x, y, z);
            var edit = this.Chunk.Edit(local);

            if (edit.Block != BlockDefOf.Soil)
                return;

            edit.Block = BlockDefOf.Grass;
            edit.Material = MaterialDefOf.Soil;
            edit.Flush();
        }

        internal override void ResolveReferences()
        {
            this.Map.Events.ListenTo<ActorEnteringNewCellEvent>(HandleActorEnteringNewCell);
            return;
            Reset();
        }

        private void Reset()
        {
            for (int i = 0; i < Chunk.Size; i++)
            {
                for (int j = 0; j < Chunk.Size; j++)
                {
                    var cell = this.Chunk.Query(new IntVec3Local(i, j, this.Chunk.HeightMap[i][j]));
                    if (cell.Block.BlockDef == BlockDefOf.Grass)
                        cell.Block = BlockDefOf.Soil.Block;
                }
            }
        }
    }
}
