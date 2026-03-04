using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Components.Plants;
using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Plants;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Terrain
{
    internal abstract class ChunkController(Chunk chunk)
    {
        protected Chunk Chunk = chunk;

        internal virtual void ResolveReferences() { }

        internal virtual void Tick() { }
    }
    internal sealed class FlowerController(Chunk chunk) : ChunkController(chunk)
    {
        const float TargetDensity = .2f;

        readonly HashSet<IntVec2> DirtyColumns = [];
        readonly Dictionary<IntVec2, int> GrassPerColumn = [];
        readonly Dictionary<IntVec2, int> FlowersPerColumn = [];
        readonly BlockGrass Grass = BlockDefOf.Grass.Block as BlockGrass;
        ulong NextTick;

        MapBase Map => this.Chunk.Map;
        Random Rand => field ??= this.Map.Random;
        float CurrentDensity => (float)this.FlowersPerColumn.Count / (this.FlowersPerColumn.Count + this.GrassPerColumn.Count);
        bool IsSaturated => this.CurrentDensity >= TargetDensity;

        private void Reschedule()
            //=> this.NextTick = this.Chunk.Map.World.CurrentTick + (ulong)this.Chunk.Map.Random.Next(Ticks.FromHours(1), Ticks.FromHours(2));
        => this.NextTick = this.Chunk.Map.World.CurrentTick + (ulong) this.Chunk.Map.Random.Next(1, 2);

        internal void ResolveReferences()
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

        internal override void Tick()
        {
            if (this.Chunk.Map.Net.IsClient)
                return;
            this.ScanDirtyColumns(this.Chunk);
            this.TrySpawnFlower();
        }

        private bool OnSchedule()
        {
            if (this.Chunk.Map.World.CurrentTick >= this.NextTick)
            {
                Reschedule();
                return true;
            }
            return false;
        }

        void TrySpawnFlower()
        {
            if (!this.OnSchedule())
                return;
            if (this.IsSaturated)
                return;
            if (this.FlowersPerColumn.Count + this.GrassPerColumn.Count == 0)
                return;
            var map = this.Chunk.Map;
            var densityGap = TargetDensity - this.CurrentDensity;
            var spawnChance = Math.Clamp(densityGap * densityGap / TargetDensity, 0, 1);
            var spreadChance = 1 - spawnChance;
            var random = this.Rand;
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

            if (this.GrassPerColumn.ContainsKey(coords))
                this.GrassPerColumn.Remove(coords);
            if (this.FlowersPerColumn.ContainsKey(coords))
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
    internal sealed class FloraController(Chunk chunk) : ChunkController(chunk)
    {
        const float TargetDensity = .2f;
        HashSet<IntVec2> DirtyColumns = [];
        Dictionary<IntVec2, int> CurrentFertileCellPerColumn = [];
        HashSet<Entity> CurrentPlants = [];
        MapBase Map => this.Chunk.Map;
        float CurrentDensityNew => (float)this.CurrentPlants.Count / this.CurrentFertileCellPerColumn.Count;// MaxPossibleFertileCells;
        bool IsSaturatedNew => this.CurrentDensityNew > TargetDensity;
        bool Dirty = true;
        ulong NextTick;

        internal void ResolveReferences()
        {
            var map = this.Map;
            map.Events.ListenTo<EntitySpawnedEvent>(HandleEntitySpawned);
            map.Events.ListenTo<EntityDespawnedEvent>(HandleEntityDespawned);
            map.Events.ListenTo<CellsInvalidatedEvent>(HandleCellsInvalidated);

            this.Dirty = true;
            foreach (var c in Chunk.Columns)
                this.DirtyColumns.Add(c);
        }

        //Action<MapBase, IntVec3>[] SpawnActions => field ??= [SpawnEntity, SpawnBlock];

        PlantSpeciesDef[] AllSpieces => field ??= [.. Def.GetDefs<PlantSpeciesDef>()];

        internal override void Tick()
        {
            if (this.Chunk.Map.Net.IsClient)
                return;
            this.GeneratePlant();
        }

        public void GeneratePlant()
        {
            if (this.Dirty)
            {
                this.ScanPlants(this.Chunk);
                this.ScanDirtyColumns(this.Chunk);
                this.Dirty = false;
            }

            if (!this.OnSchedule())
                return;

            if (this.IsSaturatedNew)
                return;

            var local = this.GetRandomFertileCell();
            if (!local.HasValue)
                return;

            var global = local.Value.ToGlobal(this.Chunk);
            if (this.CanCurrentlyGrowOn(global))
                this.SpawnEntity(this.Map, global);
        }

        private bool OnSchedule()
        {
            if(this.Map.World.CurrentTick >= this.NextTick)
            {
                Reschedule();
                return true;
            }
            return false;
        }

        private void Reschedule()
            => this.NextTick = this.Map.World.CurrentTick + (ulong)this.Map.Random.Next(Ticks.FromHours(1), Ticks.FromHours(2));

        private void SpawnEntity(MapBase map, IntVec3 global)
        {
            var randomPlant = this.AllSpieces.SelectRandom(map.Random);
            var plant = randomPlant.Create(PlantStageDefOf.Plant);
            map.World.Register(plant);
            map.Spawn(plant, global.Above, Vector3.Zero);
        }

        //private void SpawnBlock(MapBase map, IntVec3 global)
        //{
        //    if (map.GetBlock(global) == BlockDefOf.Grass.Block)
        //        BlockGrass.GrowRandomFlower(map, global);
        //}

        bool CanCurrentlyGrowOn(IntVec3 global)
        {
            var above = global.Above;
            return
                !this.Map.GetEntitiesAt(above).Any();
        }

        int? ScanColumn(Chunk chunk, IntVec2 coords)
        {
            for (int z = MapBase.MaxHeight - 1; z > 0; z--)
            {
                var local = new IntVec3(coords.X, coords.Y, z);
                var cell = chunk.GetLocalCell(local);
                if (cell.Block == BlockDefOf.Air.Block)
                    continue;
                if (cell.Block.GetFertility(cell) > 0)
                    return z;
                else
                    break;
            }
            return null;
        }
        void ScanPlants(Chunk chunk)
        {
            foreach (var plant in chunk.Entities.Where(o => o.HasComponent<PlantComponent>()))
                this.CurrentPlants.Add(plant);
        }
        void ScanDirtyColumns(Chunk chunk)
        {
            foreach (var column in this.DirtyColumns)
            {
                var z = ScanColumn(chunk, column);
                if (z.HasValue)
                    this.CurrentFertileCellPerColumn[column] = z.Value;
                else if (this.CurrentFertileCellPerColumn.ContainsKey(column))
                    this.CurrentFertileCellPerColumn.Remove(column);
            }
            this.DirtyColumns.Clear();
        }
        IntVec3? GetRandomFertileCell()
        {
            var columns = this.CurrentFertileCellPerColumn.Keys;
            if (columns.Count == 0)
                return null;
            var randomColumn = columns.SelectRandom(this.Map.Random);
            return new IntVec3(randomColumn.X, randomColumn.Y, this.CurrentFertileCellPerColumn[randomColumn]);
        }
        private void HandleCellsInvalidated(CellsInvalidatedEvent e)
        {
            foreach(var cell in e.Positions)
            {
                if (!this.Chunk.Contains(cell))
                    continue;
                var local = new IntVec3Local(cell);
                this.DirtyColumns.Add(new IntVec2(local.X, local.Y));
            }    
        }
        private void HandleEntityDespawned(EntityDespawnedEvent e)
        {
            var entity = e.Entity;
            var cell = entity.Cell;
            if (!this.Chunk.Contains(cell))
                return;
            this.CurrentPlants.Remove(entity);
        }

        private void HandleEntitySpawned(EntitySpawnedEvent e)
        {
            var entity = e.Entity;
            var cell = entity.Cell;
            if (!this.Chunk.Contains(cell))
                return;
            this.CurrentPlants.Add(entity);
        }
    }
}
