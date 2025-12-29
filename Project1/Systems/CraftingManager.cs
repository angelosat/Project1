using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class CraftingManager : TownComponent
    {
        private int NextOrderId = 1;
        public override string Name => "CraftingManager";
        readonly Dictionary<IntVec3, BlockWorkstationComp> _byPosition = [];
        readonly Dictionary<WorkstationDef, List<BlockWorkstationComp>> _byType = [];
        readonly Dictionary<int, OrderSettings> _ordersById = [];
        public CraftingManager(Town town) : base(town)
        {
            var workstationDefs = Def.GetDefs<WorkstationDef>();
            foreach (var def in workstationDefs)
                this._byType.Add(def, []);
        }
        public IEnumerable<BlockWorkstationComp> AllWorkstations => this._byPosition.Values;
        
        internal override void ResolveReferences()
        {
            this.Town.Map.Events.ListenTo<BlocksUpdatedEvent>(OnBlocksUpdated);
            this.RegisterWorkstationsFromMap();
        }

        private void RegisterWorkstationsFromMap()
        {
            foreach (var comp in this.Town.Map.BlockEntities
                            .Select(e =>
                            {
                                e.Comps.TryGetComp<BlockWorkstationComp>(out var c);
                                return c;
                            })
                            .Where(c => c is not null))
            {
                foreach (var cell in comp.Parent.CellsOccupied)
                    this.RegisterWorkstation(cell, comp);
            }
        }

        public IEnumerable<OrderSettings> GetAllOrdersUnsorted()
        {
            foreach (var order in this._ordersById.Values)
                yield return order;
        }
        public IEnumerable<MaterialRefinementDef> GetRefinementsBy(WorkstationDef workstation)
        {
            return workstation.Refinements;
        }
        private void OnBlocksUpdated(BlocksUpdatedEvent changed)
        {
            var map = this.Town.Map;
            foreach (var pos in changed.Positions)
            {
                //var workstation = map.GetBlockEntity(pos)?.GetComp<BlockWorkstationComp>();
                BlockWorkstationComp workstation = default;
                if (!map.GetBlockEntity(pos)?.Comps.TryGetComp(out workstation) ?? false)
                    continue;
                if (workstation is not null)
                {
                    RegisterWorkstation(pos, workstation);
                }
                else if (this._byPosition.TryGetValue(pos, out var existing))
                {
                    this._byPosition.Remove(pos);
                    this._byType[existing.WorkstationType].Remove(existing);
                }
            }
        }

        private void RegisterWorkstation(IntVec3 pos, BlockWorkstationComp workstation)
        {
            if (this._byPosition.TryGetValue(pos, out var existing))
                this._byType[existing.WorkstationType].Remove(existing);
            this._byPosition[pos] = workstation;
            this._byType[workstation.WorkstationType].Add(workstation);
        }

        public OrderSettings CreateOrder(IntVec3 workstationPosition, MaterialRefinementDef refinement)
        {
            var workstation = this.Map.GetBlockEntity(workstationPosition) ?? throw new ArgumentException($"Block entity doesn't exist at {workstationPosition}");
            var comp = workstation.GetComp<BlockWorkstationComp>() ?? throw new ArgumentException($"{workstation} doesn't own a {nameof(BlockWorkstationComp)}");

            var order = new OrderSettings(this.NextOrderId++, comp, refinement);

            comp.Orders.Add(order);
            this._ordersById.Add(order.Id, order);

            this.Map.Events.Post(new CraftOrderAddedEvent(comp, order));
            return order;
        }

        internal OrderSettings DeleteOrder(int id)
        {
            if (!this._ordersById.TryGetValue(id, out var order)) throw new ArgumentException($"Order with id: {id} didn't exist");
            this._ordersById.Remove(id);
            var comp = this.Map.GetBlockEntity(order.Workstation.Global).GetComp<BlockWorkstationComp>();
            comp.Orders.Remove(order);
            this.Map.Events.Post(new CraftOrderRemovedEvent(comp, order));
            return order;
        }

        internal OrderSettings GetOrderBy(int id)
        {
            return this._ordersById[id];
        }

        internal float GetProgressFor(Actor actor)
        {
            throw new NotImplementedException();
        }
    }
    public class CraftOrderAddedEvent(BlockWorkstationComp comp, OrderSettings order) : EventPayloadBase
    {
        public readonly BlockWorkstationComp Comp = comp;
        public readonly OrderSettings Order = order;
    }
    public class CraftOrderRemovedEvent(BlockWorkstationComp comp, OrderSettings order) : EventPayloadBase
    {
        public readonly BlockWorkstationComp Comp = comp;
        public readonly OrderSettings Order = order;
    }
    public class CraftOrderModifiedEvent(OrderSettings order) : EventPayloadBase
    {
        public readonly OrderSettings Order = order;
    }
    public class CraftOrderReorderedEvent(OrderSettings order) : EventPayloadBase
    {
        public readonly OrderSettings Order = order;
    }
}
