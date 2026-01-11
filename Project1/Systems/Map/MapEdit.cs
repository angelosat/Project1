using SharpDX.Direct2D1.Effects;
using SharpDX.Direct3D9;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Start_a_Town_
{
    internal interface ICellChangeRecorder
    {
        void Record(IntVec3 global, SetBlockArgs args);
        void AddEntity(BlockEntity entity);
        void RemoveEntity(BlockEntity entity);
    }
    internal class MapEdit(MapBase map) : ICellChangeRecorder
    {
        readonly Dictionary<IntVec3, SetBlockArgs> Changes = [];
        readonly HashSet<BlockEntity> EntitiesAdded = [];
        readonly HashSet<BlockEntity> EntitiesRemoved = [];
        readonly Dictionary<BlockEntity, List<IntVec3>> CellsToAttach = [];
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
            this.Map.Events.Post(new BlocksChangedEvent(this.Map, this.Changes.Values));
            this.Map.Events.Post(new BlocksUpdatedEvent(this.Map, this.Changes.Keys));
            foreach (var entity in this.EntitiesRemoved)
            {
                this.Map.RemoveBlockEntityInternal(entity);
                this.Map.Events.Post(new BlockEntityRemovedEvent(entity));
            }
            foreach (var entity in this.EntitiesAdded)
            {
                this.Map.AddBlockEntityInternal(entity);
                this.Map.Events.Post(new BlockEntityAddedEvent(entity));
            }
        }
        internal void Paint(IEnumerable<IntVec3> targets, Block block, MaterialDef material, byte data, int variation, int orientation)
        {
            foreach (var cell in targets)
                this.Remove(cell);
            foreach (var cell in targets)
            {
                var cellmutations = new List<(IntVec3, byte)>();
                var plan = block.GetFootprint(this.Map, cell, orientation);
                var first = plan.First();
                var origin = first.global;
                if (block.TryLinkToAdjacentBlockEntity(this.Map, cell) is BlockEntity entityExisting)
                    this.RecordAttachCellToEntity(cell, entityExisting);
                //else if(block.BlockDef.CreateEntity(cell) is BlockEntity entity)
                //    this.EntitiesAdded.Add(entity);
                else if (block.BlockDef.CreateEntity() is BlockEntity entity)
                    this.EntitiesAdded.Add(entity.SetFootprint(plan.Select(c => c.global)));
                foreach (var target in plan)
                    this.Changes[target.global] = new SetBlockArgs(target.global, block, material, target.data, orientation, origin - target.global);
                //this.Place(cell, block.BlockDef, material, data, variation, orientation);
            }
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
                RecordAttachCellToEntity(global, entity);
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

        private void RecordAttachCellToEntity(IntVec3 global, BlockEntity entity)
        {
            if (!this.CellsToAttach.TryGetValue(entity, out var list))
                this.CellsToAttach[entity] = list = [];
            list.Add(global);
        }

        internal static void Paint(MapBase map, IEnumerable<IntVec3> targets, Block block, MaterialDef material, byte data, int variation, int orientation)
        {
            var op = new MapEdit(map);
            op.Paint(targets, block, material, data, variation, orientation);
            op.Flush();
        }
    }
    internal class CellOperationContext //{ Simulation, Dev }
    {

    }
}
