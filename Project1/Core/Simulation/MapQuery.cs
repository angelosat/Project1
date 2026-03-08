using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Materials;
using Project1.Framework;
using System;
using System.Collections.Generic;

namespace Project1.Core.Simulation
{
    public record MapQuery(MapBase Map, IntVec3 Global)
    {
        readonly CellId CellIndex = Chunk.GetCellIndex(Global);
        readonly Chunk Chunk = Map.GetChunk(Global);
        public Block Block => this.Chunk.GetBlock(this.CellIndex);
        public MaterialDef Material => this.Chunk.GetMaterial(this.CellIndex);
        [Obsolete]
        public Cell GetCell() => this.Map.GetCell(this.Global);
        public IEnumerable<Entity> GetEntities() => this.Map.GetEntitiesAt(this.Global);
        
        public BlockEntity GetBlockEntity() => this.Map.GetBlockEntity(this.Global);
        public IEnumerable<(IntVec3 cell, byte data)> GetBlockFootprint()
        {
            var cell = this.GetCell();
            return cell.Block.GetFootprint(this.Map, this.Global, cell.Orientation);
        }
        public CellQuery CellQuery => new(this.Chunk, this.Global);
        public MapQuerySnapshot ToSnapshot() 
            => !this.Map.Contains(this.Global) ? 
            default : 
            new([.. this.GetEntities()], this.GetCell(), this.GetBlockEntity());
    }
    public record struct MapQuerySnapshot(List<Entity> Entities, Cell Cell, BlockEntity BlockEntity) { }
}
