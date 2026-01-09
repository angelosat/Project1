using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    internal interface ICellChangeRecorder
    {
        void Record(IntVec3 global, SetBlockArgs args);
        void AddEntity(BlockEntity entity);
        void RemoveEntity(BlockEntity entity);
    }
    internal class CellOperation(MapBase map) : ICellChangeRecorder
    {
        Dictionary<IntVec3, SetBlockArgs> Changes = [];
        HashSet<BlockEntity> EntitiesAdded = [], EntitiesRemoved = [];
        Dictionary<BlockEntity, List<IntVec3>> CellsToAttach = [];
        MapBase Map = map;
        CellOperationContext Context;
        public void Record(IntVec3 global, SetBlockArgs args)
        {
            this.Changes[global] = args;
        }
        public void AddEntity(BlockEntity entity) => this.EntitiesAdded.Add(entity);
        public void RemoveEntity(BlockEntity entity) => this.EntitiesRemoved.Add(entity);
        internal void Flush()
        {
            foreach(var (entity, cells) in this.CellsToAttach)
                foreach (var cell in cells)
                    entity.Attach(cell);
            this.Map.SetBlockInternal(this.Changes);
            foreach (var entity in this.EntitiesRemoved)
                this.Map.RemoveBlockEntityInternal(entity);
            foreach (var entity in this.EntitiesAdded)
                this.Map.AddBlockEntityInternal(entity);
        }
        internal void Paint(IEnumerable<IntVec3> targets, Block block, MaterialDef material, byte data, int variation, int orientation)
        {
            foreach(var cell in targets)
                this.Remove(cell);
            foreach(var cell in targets)
                this.Place(cell, block.BlockDef, material, data, variation, orientation);
        }
        void Remove(IntVec3 global)
        {
            if (this.Map.GetBlockEntity(global) is BlockEntity entity)
            {
                this.EntitiesRemoved.Add(entity);
                foreach(var c in entity.CellsOccupied)
                    this.Changes[c] = new SetBlockArgs(c, BlockDefOf.Air.Worker, MaterialDefOf.Air, 0, 0, IntVec3.Zero);

                // blockentity gets precedent for which cells it occupies, so we dont have to further check for simple multicelled blocks
                return;
            }
            var cell = this.Map.GetCell(global);
            var parts = cell.GetParts(global);
            foreach (var p in parts)
                this.Changes[p] = new SetBlockArgs(p, BlockDefOf.Air.Worker, MaterialDefOf.Air, 0, 0, IntVec3.Zero);
        }

        void Place(IntVec3 global, BlockDef block, MaterialDef material, byte data, int variation, int orientation)
        {
            var worker = block.Worker;
            var map = this.Map;
            if (worker.TryLinkToAdjacentBlockEntity(map, global) is BlockEntity entity)
            {
                if(!this.CellsToAttach.TryGetValue(entity, out var list))
                    this.CellsToAttach[entity] = list = [];
                list.Add(global);
            }
            else
            {
                entity = block.CreateEntity(global);
                if (entity is not null)
                    this.EntitiesAdded.Add(entity);
            }
            // todo: set source correctly
            this.Changes[global] = new SetBlockArgs(global, worker, material, data, orientation, IntVec3.Zero);
        }
    }
    internal class CellOperationContext //{ Simulation, Dev }
    {

    }
}
