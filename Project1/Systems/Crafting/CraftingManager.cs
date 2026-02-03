using Project1.Framework.Base;
using Project1.Framework.Blocks;
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
        readonly Dictionary<WorkstationDef, HashSet<BlockWorkstationComp>> _byType = [];
        readonly Dictionary<int, OrderSettings> _ordersById = [];
        readonly Dictionary<BlockEntity, OrderSettings> _ordersByWorkstation = [];
        public IEnumerable<BlockWorkstationComp> AllWorkstations => this._byType.SelectMany(d => d.Value);
        public IEnumerable<IGrouping<BlockEntity, List<OrderSettings>>> OrdersByWorkstation => AllWorkstations.GroupBy(i => i.Parent, i => i.Orders);
        public CraftingManager(Town town) : base(town)
        {
            var workstationDefs = Def.GetDefs<WorkstationDef>();
            foreach (var def in workstationDefs)
                this._byType.Add(def, []);
        }
        public IEnumerable<BlockWorkstationComp> AllWorkstationModules => this._byPosition.Values;

        internal override void ResolveReferences()
        {
            this.Town.Map.Events.ListenTo<CellsInvalidatedEvent>(OnBlocksUpdated);
            this.Town.Map.Events.ListenTo<BlockEntityRemovedEvent>(OnBlockEntityRemoved);
            this.Town.Map.Events.ListenTo<BlockEntityAddedEvent>(OnBlockEntityAdded);
            this.ScanWorkstations();
            this.ScanOrders();
        }

        private void ScanWorkstations()
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
        void ScanOrders()
        {
            foreach (var workstation in this._byType.SelectMany(c => c.Value))
                foreach (var order in workstation.Orders)
                {
                    this._ordersById.Add(order.Id, order);
                    this.NextOrderId = Math.Max(this.NextOrderId, order.Id + 1);
                }
        }
        void RegisterOrder(OrderSettings order)
        {
            this._ordersById.Add(order.Id, order);
            this._ordersByWorkstation.Add(order.Workstation.Parent, order);
        }
        public IEnumerable<OrderSettings> GetAllOrdersUnsorted()
        {
            foreach (var order in this._ordersById.Values)
                yield return order;
        }
        //public IEnumerable<MaterialRefinementDef> GetRefinementsBy(WorkstationDef workstation)
        //{
        //    return workstation.Refinements;
        //}
        private void OnBlocksUpdated(CellsInvalidatedEvent changed)
        {
            //var map = this.Town.Map;
            //foreach (var pos in changed.Positions)
            //    if (this._byPosition.TryGetValue(pos, out var existing))
            //    {
            //        this._byPosition.Remove(pos);
            //        this._byType[existing.WorkstationType].Remove(existing);
            //    }
        }
        private void OnBlockEntityAdded(BlockEntityAddedEvent e)
        {
            if (e.Entity.Def.Worker is not BlockWorkstation)
                return;
            this.RegisterWorkstation(e.Entity.GetComp<BlockWorkstationComp>());
        }
        private void OnBlockEntityRemoved(BlockEntityRemovedEvent e)
        {
            if (e.Entity.Def.Worker is not BlockWorkstation)
                return;
            this.UnregisterWorkstation(e.Entity.GetComp<BlockWorkstationComp>());
        }

        private bool UnregisterWorkstation(BlockWorkstationComp comp)
        {
            this._byType[comp.WorkstationType].Remove(comp);
            foreach (var pos in comp.Parent.CellsOccupied)
                this._byPosition.Remove(pos);
            foreach (var order in comp.Orders)
                this._ordersById.Remove(order.Id);
            return true;
        }

        private void RegisterWorkstation(IntVec3 pos, BlockWorkstationComp workstation)
        {
            if (this._byPosition.TryGetValue(pos, out var existing))
                this._byType[existing.WorkstationType].Remove(existing);
            this._byPosition[pos] = workstation;
            this._byType[workstation.WorkstationType].Add(workstation);
        }
        private void RegisterWorkstation(BlockWorkstationComp workstation)
        {
            var entity = workstation.Parent;
            foreach(var cell in entity.CellsOccupied)
                //this._byPosition.Add(cell, workstation);
                this._byPosition[cell] = workstation;

            this._byType[workstation.WorkstationType].Add(workstation);
        }
        public OrderSettings CreateOrderNew(IntVec3 workstationPosition, Def recipe)
        {
            var workstation = this.Map.GetBlockEntity(workstationPosition) ?? throw new ArgumentException($"Block entity doesn't exist at {workstationPosition}");
            var comp = workstation.GetComp<BlockWorkstationComp>() ?? throw new ArgumentException($"{workstation} doesn't own a {nameof(BlockWorkstationComp)}");

            var reqs = CraftingSystem.GetValidIngredientsPerSlot(recipe);
            if (reqs.Count() > workstation.CellsOccupied.Count)
            {
                Log.Error($"Not enough workstation modules to craft {recipe.Label}");
                return null;
            }
            var order = new OrderSettings(this.NextOrderId++, comp, recipe);

            comp.Orders.Add(order);
            this._ordersById.Add(order.Id, order);

            this.Map.Events.Post(new CraftOrderAddedEvent(comp, order));
            return order;
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

        internal OrderSettings GetOrder(int id)
        {
            return this._ordersById[id];
        }

        internal float GetProgressFor(Actor actor)
        {
            throw new NotImplementedException();
        }
    }
    public record struct CraftOrderAddedEvent(BlockWorkstationComp Comp, OrderSettings Order) : IEventPayload { }
    public record struct CraftOrderRemovedEvent(BlockWorkstationComp Comp, OrderSettings Order) : IEventPayload { }
    public record struct CraftOrderUpdatedEvent(OrderSettings Order) : IEventPayload { }
    public record struct CraftOrderReorderedEvent(OrderSettings Order) : IEventPayload { }
}
