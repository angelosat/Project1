using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class CraftingManager : TownComponent
    {
        private int NextOrderId = 1;
        public override string Name => "CraftingManager";
        readonly Dictionary<IntVec3, BlockEntityCompWorkstation> _byPosition = [];
        readonly Dictionary<WorkstationDef, List<BlockEntityCompWorkstation>> _byType = [];
        readonly Dictionary<int, OrderSettings> _ordersById = [];
        public CraftingManager(Town town) : base(town)
        {
            var workstationDefs = Def.GetDefs<WorkstationDef>();
            foreach (var def in workstationDefs)
                this._byType.Add(def, []);
        }
        internal override void ResolveReferences()
        {
            this.Town.Map.Events.ListenTo<BlocksUpdatedEvent>(OnBlocksUpdated);
        }
        public IEnumerable<MaterialMappingDef> GetProcessesFor(WorkstationDef workstation)
        {
            return workstation.Processes;
        }
        private void OnBlocksUpdated(BlocksUpdatedEvent changed)
        {
            var map = this.Town.Map;
            foreach (var pos in changed.Positions)
            {
                var workstation = map.GetBlockEntity(pos)?.GetComp<BlockEntityCompWorkstation>();
                if (workstation is not null)
                {
                    if (this._byPosition.TryGetValue(pos, out var existing))
                        this._byType[existing.Type].Remove(existing);
                    this._byPosition[pos] = workstation;
                    this._byType[workstation.Type].Add(workstation);
                }
                else if (this._byPosition.TryGetValue(pos, out var existing))
                {
                    this._byPosition.Remove(pos);
                    this._byType[existing.Type].Remove(existing);
                }
            }
        }
        public OrderSettings CreateOrder(IntVec3 workstationPosition, MaterialMappingDef process)
        {
            var workstation = this.Map.GetBlockEntity(workstationPosition) ?? throw new ArgumentException($"Block entity doesn't exist at {workstationPosition}");
            var comp = workstation.GetComp<BlockEntityCompWorkstation>() ?? throw new ArgumentException($"{workstation} doesn't own a {nameof(BlockEntityCompWorkstation)}");

            var order = new OrderSettings(this.NextOrderId++, comp, process);

            comp.Orders.Add(order);
            this._ordersById.Add(order.Id, order);

            this.Map.Events.Post(new CraftOrderAddedEvent(comp, order));
            return order;
        }

        internal OrderSettings DeleteOrder(int id)
        {
            if (!this._ordersById.TryGetValue(id, out var order)) throw new ArgumentException($"Order with id: {id} didn't exist");
            this._ordersById.Remove(id);
            var comp = this.Map.GetBlockEntity(order.Owner.Global).GetComp<BlockEntityCompWorkstation>();
            comp.Orders.Remove(order);
            this.Map.Events.Post(new CraftOrderRemovedEvent(comp, order));
            return order;
        }
    }
    public class CraftOrderAddedEvent(BlockEntityCompWorkstation comp, OrderSettings order) : EventPayloadBase
    {
        public readonly BlockEntityCompWorkstation Comp = comp;
        public readonly OrderSettings Order = order;
    }
    public class CraftOrderRemovedEvent(BlockEntityCompWorkstation comp, OrderSettings order) : EventPayloadBase
    {
        public readonly BlockEntityCompWorkstation Comp = comp;
        public readonly OrderSettings Order = order;
    }
}
