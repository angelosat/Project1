using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Simulation
{
    public record MapQuery(MapBase Map, IntVec3 Global)
    {
        public IEnumerable<Entity> GetEntities() => this.Map.GetEntitiesAt(this.Global);
        public Cell GetCell() => this.Map.GetCell(this.Global);
        public BlockEntity GetBlockEntity() => this.Map.GetBlockEntity(this.Global);
        public MapQuerySnapshot Query() => new([.. this.GetEntities()], this.GetCell(), this.GetBlockEntity());
    }
    public record struct MapQuerySnapshot(List<Entity> Entities, Cell Cell, BlockEntity BlockEntity) { }
}
