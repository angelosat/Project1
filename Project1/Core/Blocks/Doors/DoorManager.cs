using System.Collections.Generic;
using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core.Blocks.Doors
{
    internal class DoorManager : MapComponent
    {
        readonly HashSet<BlockDoorComp> Comps = [];
        readonly Dictionary<IntVec3, BlockDoorComp> CellsToDoors = [];
        public DoorManager(MapBase map) : base(map)
        {
            map.Events.ListenTo<BlockEntityAddedEvent>(OnBlockEntityAdded);
            map.Events.ListenTo<BlockEntityRemovedEvent>(OnBlockEntityRemoved);
        }
        private void OnBlockEntityAdded(BlockEntityAddedEvent e)
        {
            if(e.Entity.Comps.TryGetComp<BlockDoorComp>(out var comp))
                this.RegisterComp(comp);
        }
        private void OnBlockEntityRemoved(BlockEntityRemovedEvent e)
        {
            if (e.Entity.Comps.TryGetComp<BlockDoorComp>(out var comp))
                this.UnregisterComp(comp);
        }
        protected internal override void ResolveReferences()
        {
            foreach (var entity in this.Map.BlockEntities)
                if (entity.Comps.TryGetComp<BlockDoorComp>(out var comp))
                    this.RegisterComp(comp);
        }
        void RegisterComp(BlockDoorComp comp)
        {
            this.Comps.Add(comp);
            foreach (var cell in comp.Parent.CellsOccupied)
                this.CellsToDoors[cell] = comp;
        }
        void UnregisterComp(BlockDoorComp comp)
        {
            this.Comps.Remove(comp);
            foreach (var cell in comp.Parent.CellsOccupied)
                this.CellsToDoors.Remove(cell);
        }
        public override void Tick()
        {
        }
    }
}
