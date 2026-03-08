using Project1.Core.Blocks;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Terrain
{
    internal sealed class FlowerController(Chunk chunk) : ChunkController(chunk)
    {
        const float TargetDensity = .2f;

        readonly HashSet<IntVec2> DirtyColumns = [];
        readonly Dictionary<IntVec2, int> GrassPerColumn = [];
        readonly Dictionary<IntVec2, int> FlowersPerColumn = [];
        readonly BlockGrass Grass = BlockDefOf.Grass.Block as BlockGrass;
   
        float CurrentDensity => (float)this.FlowersPerColumn.Count / (this.FlowersPerColumn.Count + this.GrassPerColumn.Count);
        bool IsSaturated => this.CurrentDensity >= TargetDensity;

        protected override int MinTicks => Ticks.FromHours(1);

        protected override int MaxTicks => Ticks.FromHours(2);

        internal override void ResolveReferences()
        {
            this.Chunk.Map.Events.ListenTo<CellsInvalidatedEvent>(HandleCellsInvalidated);

            foreach (var c in Chunk.Columns)
                this.DirtyColumns.Add(c);
            this.ScanDirtyColumns(this.Chunk);//, true);
        }

        private void HandleCellsInvalidated(CellsInvalidatedEvent e)
        {
            foreach(var global in e.Positions)
            {
                if (!this.Chunk.Contains(global))
                    continue;
                var local = global.ToLocal();
                if (this.GrassPerColumn.TryGetValue(local.XY, out var gz) && local.Z >= gz)
                    this.DirtyColumns.Add(local.XY);
                else if (this.FlowersPerColumn.TryGetValue(local.XY, out var fz) && local.Z >= fz)
                    this.DirtyColumns.Add(local.XY);
            }
        }

        protected override void ScheduledTick()
        {
            this.ScanDirtyColumns(this.Chunk);
            this.TrySpawnFlower();
        }
        void TrySpawnFlower()
        {
            if (this.IsSaturated)
                return;
            if (this.FlowersPerColumn.Count + this.GrassPerColumn.Count == 0)
                return;
            var map = this.Chunk.Map;
            var densityGap = TargetDensity - this.CurrentDensity;
            var spawnChance = Math.Clamp(densityGap * densityGap / TargetDensity, 0, 1);
            var spreadChance = 1 - spawnChance;
            var random = this.Random;
            var roll = random.NextDouble();
            if (roll < spawnChance)
            {
                if (this.GrassPerColumn.Count == 0)
                    return;

                // SPAWN NEW FLOWER (isolated)
                var column = this.GrassPerColumn.Keys.SelectRandom(random);
                var local = new IntVec3(column, this.GrassPerColumn[column]);
                var global = local.ToGlobal(this.Chunk);

                // Only spawn if no adjacent flowers
                if (global.GetAdjacentCubeLazy().Any(c => map.Contains(c) && map.Query(c).Cell.BlockData > 0))
                    return;
                this.Chunk.SetBlockData(global, BlockGrass.GetRandomFlower(map));
            }
            else
            {
                if (this.FlowersPerColumn.Count == 0)
                    return;

                // SPREAD FROM EXISTING FLOWER
                var column = this.FlowersPerColumn.Keys.SelectRandom(random);
                var flowerLocal = new IntVec3(column, this.FlowersPerColumn[column]);
                var flowerGlobal = flowerLocal.ToGlobal(this.Chunk);
                var flowerData = this.Chunk.GetBlockData(flowerLocal);

                // Find valid adjacent grass cells with sunlight
                var candidates = flowerGlobal.GetAdjacentCubeLazy()
                    .Where(c => 
                        map.Query(c).Cell is Cell cell && 
                        cell.Block == this.Grass && 
                        cell.BlockData == 0 &&
                        this.Chunk.GetSunlightPercentage(c.Above) >= .5f
                    ).ToList();

                // Only spread if coarseness threshold met
                if (candidates.Count < CubeCountThreshold(cubeCount: 26)) // e.g., 26 neighbors
                    return;

                var selectedGlobal = candidates.SelectRandom(map.Random);
                this.Chunk.SetBlockData(selectedGlobal, flowerData);
            }
        }

        // Helper for coarseness threshold
        static int CubeCountThreshold(int cubeCount)
            => cubeCount / 6;
        
        void ScanDirtyColumns(Chunk chunk, bool init = false)
        {
            foreach (var column in this.DirtyColumns)
                ScanColumn(chunk, column, init);
            this.DirtyColumns.Clear();
        }

        void ScanColumn(Chunk chunk, IntVec2 coords, bool init = false)
        {
            //var grass = BlockDefOf.Grass.Block as BlockGrass;

            this.GrassPerColumn.Remove(coords);
            this.FlowersPerColumn.Remove(coords);

            for (int z = MapBase.MaxHeight - 1; z > 0; z--)
            {
                var local = new IntVec3(coords.X, coords.Y, z);
                var cell = chunk.GetLocalCell(local);
                if (cell.Block == this.Grass)
                {
                    if (init)
                        cell.BlockData = 0;
                    if (this.Grass.HasFlower(cell.BlockData))
                        FlowersPerColumn[coords] = z;
                    else
                        GrassPerColumn[coords] = z;
                }
               
            }
        }
    }
}
