using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Components.Plants;
using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Plants;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Terrain
{
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

        internal override void ResolveReferences()
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

        protected override int MinTicks => Ticks.FromHours(1);

        protected override int MaxTicks => Ticks.FromHours(2);

        protected override void ScheduledTick()
        {
            //if (this.Chunk.Map.Net.IsClient)
            //    return;
            this.GeneratePlant();
        }

        void GeneratePlant()
        {
            if (this.Dirty)
            {
                this.ScanPlants(this.Chunk);
                this.ScanDirtyColumns(this.Chunk);
                this.Dirty = false;
            }

            //if (!this.OnSchedule())
            //    return;

            if (this.IsSaturatedNew)
                return;

            var local = this.GetRandomFertileCell();
            if (!local.HasValue)
                return;

            var global = local.Value.ToGlobal(this.Chunk);
            if (this.CanCurrentlyGrowOn(global))
                this.SpawnEntity(this.Map, global);
        }

        //private bool OnSchedule()
        //{
        //    if(this.Map.World.CurrentTick >= this.NextTick)
        //    {
        //        Reschedule();
        //        return true;
        //    }
        //    return false;
        //}

        //private void Reschedule()
        //    => this.NextTick = this.Map.World.CurrentTick + (ulong)this.Map.Random.Next(Ticks.FromHours(1), Ticks.FromHours(2));

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
