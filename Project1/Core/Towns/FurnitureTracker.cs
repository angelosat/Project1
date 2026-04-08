using Project1.Core.Blocks;
using Project1.Core.Rooms;
using Project1.Core.Simulation;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns
{
    public class FurnitureTracker(Town town) : TownComp(town)
    {
        public override string Name => "Furniture";

        readonly Dictionary<FurnitureDef, HashSet<GlobalCellId>> _cache = [];

        public IEnumerable<IntVec3> GetFurniture(FurnitureDef fd) => this._cache[fd].Select(id => new IntVec3(id));
        public bool IsFurniture(IntVec3 global, FurnitureDef fd)
        {
            if (this._cache.TryGetValue(fd, out var list))
                return list.Contains(global);
            return false;
        }
        internal override void ResolveReferences()
        {
            this.Map.Events.ListenTo<BlocksChangedEvent>(HandleBlocksChanged);

            this.Initialize();
        }

        private void HandleBlocksChanged(BlocksChangedEvent e)
        {
            foreach(var pos in e.Changes)
            {
                this.Remove(pos.Global);
                if (pos.Block.BlockDef.Furniture is FurnitureDef fd)
                    this.Add(fd, pos.Global);
            }
        }

        private void Initialize()
        { 
            foreach(var (chunk, cell, id) in this.Map.GetAllCellsWithIndex())
            {
                if (cell.Block.BlockDef.Furniture is not FurnitureDef fd)
                    continue;
                this.Add(fd, id.Local.ToGlobal(chunk));
            }
        }
        void Add(FurnitureDef def, IntVec3 cell)
        {
            if (!this._cache.TryGetValue(def, out var list))
                this._cache[def] = list = [];
            list.Add(cell);
        }
        void Remove(IntVec3 cell)
        {
            foreach(var def in this._cache.Keys)
            {
                var list = this._cache[def];
                list.Remove(cell);
                if (list.Count == 0)
                    this._cache.Remove(def);
            }
        }
    }
}
