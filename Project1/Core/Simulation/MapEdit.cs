using System.Collections.Generic;
using System.Linq;
using Project1.Framework;
using Project1.Core.Materials;
using Project1.Core.Blocks;
using Project1.Core.Networking.Simulation;
using System.Runtime.Serialization.DataContracts;

namespace Project1.Core.Simulation
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
        readonly MapBase Map = map;
        MapEditContext Context;
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

            // fire events
            this.Map.Events.Post(new BlocksChangedEvent(this.Map, this.Changes.Values));
            this.Map.Events.Post(new CellsInvalidatedEvent(this.Map, this.Changes.Keys));
            foreach (var entity in this.EntitiesRemoved)
                this.Map.Events.Post(new BlockEntityRemovedEvent(entity));
            foreach (var entity in this.EntitiesAdded)
                this.Map.Events.Post(new BlockEntityAddedEvent(entity));
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
                else if (block.TryCreateNewBlockEntity(map, cell, orientation) is BlockEntity entity)
                    this.EntitiesAdded.Add(entity);
                foreach (var target in plan)
                    this.Changes[target.global] = new SetBlockArgs(target.global, block, material, target.data, orientation, origin - target.global);
            }
        }
        internal void PaintWithOrigin(HashSet<IntVec3> footprint, Block block, MaterialDef material, byte data, int variation, int orientation)
        {
            var origin = footprint.First();
            foreach (var cell in footprint)
                this.Changes[cell] = new SetBlockArgs(cell, block, material, data, orientation, origin - cell);
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
        internal static void Paint(MapEditContext context, MapBase map, IEnumerable<IntVec3> targets, BlockDef blockDef, MaterialDef material, byte data, int variation, int orientation)
        {
            Paint(context, map, targets, blockDef.Worker, material, data, variation, orientation);
        }
        internal static void Paint(MapEditContext context, MapBase map, IEnumerable<IntVec3> targets, Block block, MaterialDef material, byte data, int variation, int orientation)
        {
            var op = new MapEdit(map);
            op.Paint(targets, block, material, data, variation, orientation);
            op.Flush();
            map.Events.Post(new MapEditEvent(context, MapEditType.Create, map, [.. targets], block, material, data, variation, orientation));

        }
        internal static void PaintWithOrigin(MapEditContext context, MapBase map, HashSet<IntVec3> targets, Block block, MaterialDef material, byte data, int variation, int orientation)
        {
            var op = new MapEdit(map);
            op.PaintWithOrigin(targets, block, material, data, variation, orientation);
            op.Flush();
            map.Events.Post(new MapEditEvent(context, MapEditType.Replace, map, targets, block, material, data, variation, orientation));
        }

    }

    internal enum MapEditContext { Simulation, Player }
    internal enum MapEditType { Create, Replace }
}
