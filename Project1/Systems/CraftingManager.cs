using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class CraftingManager : TownComponent
    {
        public override string Name => "CraftingManager";
        readonly Dictionary<IntVec3, BlockEntityCompWorkstation> _byPosition = [];
        readonly Dictionary<WorkstationDef, List<BlockEntityCompWorkstation>> _byType = [];
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

            var order = new OrderSettings(comp, process);

            comp.Orders.Add(order);

            this.Map.Events.Post(new CraftOrderCreatedEvent(comp, order));
            return order;
        }
        
    }
    public class CraftOrderCreatedEvent(BlockEntityCompWorkstation comp, OrderSettings order) : EventPayloadBase
    {
        public readonly BlockEntityCompWorkstation Comp = comp;
        public readonly OrderSettings Order = order;
    }
}
