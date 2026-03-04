using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Materials;
using Project1.Core.Simulation;

namespace Project1.Core.Towns.Terrain
{
    internal sealed class GrassController(Chunk chunk) : ChunkController(chunk)
    {
        const double TramplingChance = 0.1f;

        //protected override int MinTicks => Ticks.FromHours(1);

        //protected override int MaxTicks => Ticks.FromHours(2);

        protected override int MinTicks => 1;

        protected override int MaxTicks => 2;

        private void HandleActorEnteringNewCell(ActorEnteringNewCellEvent e)
        {
            if (e.Actor.Net.IsClient)
                return;
            var cell = e.Actor.Cell.Below;
            if (!this.Chunk.Contains(cell))
                return;
            var query = this.Chunk.Query(cell);

            if (query.Block.BlockDef != BlockDefOf.Grass)
                return;
            var roll = this.Random.NextSingle();
            if (roll > TramplingChance)
                return;
            query.Block = BlockDefOf.Soil.Block;
        }

        protected override void ScheduledTick()
        {
            var heightMap = this.Chunk.HeightMap;
            var x = this.Random.Next(Chunk.Size);
            var y = this.Random.Next(Chunk.Size);
            var z = heightMap[x][y];
            var local = new IntVec3Local(x, y, z);
            var query = this.Chunk.Query(local);
            if (query.Block.BlockDef != BlockDefOf.Soil)
                return;
            
            var global = local.ToGlobal(this.Chunk);
            //MapEdit.Paint(MapEditContext.Simulation, this.Map, [global], BlockDefOf.Grass.Block, MaterialDefOf.Soil, 0, 0, 0);
            query.Block = BlockDefOf.Grass.Block;
            query.Material = MaterialDefOf.Soil;
        }

        internal override void ResolveReferences()
        {
            this.Map.Events.ListenTo<ActorEnteringNewCellEvent>(HandleActorEnteringNewCell);
            //return;
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
