using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class StockpileManager : MapComponent
    {
        readonly Dictionary<IntVec3, Stockpile> CellsToStockpiles = [];
        readonly List<Stockpile> _allStockpiles = [];
        readonly Dictionary<ZoneId, Stockpile> _allStockpilesById = [];
        public IReadOnlyList<Stockpile> Stockpiles => this._allStockpiles;
        public IEnumerable<Entity> AllItems => this._allStockpiles.SelectMany(s => s.Items);
        public IEnumerable<Entity> GetItems(ZoneId stockpileId) => stockpileId != ZoneId.Null ? this._allStockpilesById[stockpileId].Items : this.AllItems;
        public StockpileManager(MapBase map) : base(map)
        {
            map.Events.ListenTo<ZoneCreatedEvent>(OnZoneCreated);
            map.Events.ListenTo<ZoneDeletedEvent>(OnZoneDeleted);

            map.Events.ListenTo<EntitySpawnedEvent>(OnEntitySpawned);
            map.Events.ListenTo<EntityDespawnedEvent>(OnEntityDespawned);
            map.Events.ListenTo<EntityAtRestEvent>(OnEntityAtRest);
        }

        protected internal override void ResolveReferences()
        {
            var stockpiles = this.Map.Town.ZoneManager.GetZones<Stockpile>();
            foreach (var s in stockpiles)
            {
                this._allStockpiles.Add(s);
                this._allStockpilesById[s.ID] = s;
                foreach (var cell in s.Positions)
                    this.CellsToStockpiles[cell] = s;
            }
            foreach (var entity in this.Map.Entities)
                if (this.CellsToStockpiles.TryGetValue(entity.Cell, out var stockpile))
                    if (stockpile.Accepts(entity))
                        stockpile.AddItem(entity);
        }
        private void OnEntitySpawned(EntitySpawnedEvent e)
        {
            var supportCell = e.Entity.Cell.Below;
            if (!this.CellsToStockpiles.TryGetValue(supportCell, out var stockpile))
                return;
            if (!stockpile.Accepts(e.Entity))
                return;
            stockpile.AddItem(e.Entity);
        }
        private void OnEntityDespawned(EntityDespawnedEvent e)
        {
            var supportCell = e.Entity.Cell.Below;
            if (!this.CellsToStockpiles.TryGetValue(supportCell, out var stockpile))
                return;
            stockpile.RemoveItem(e.Entity);
        }
        private void OnEntityAtRest(EntityAtRestEvent e)
        {
            var cell = e.Entity.Cell;
            if (this.CellsToStockpiles.TryGetValue(cell, out var stockpile))
            {
                if (e.AtRest)
                    stockpile.AddItem(e.Entity);
                else
                    stockpile.RemoveItem(e.Entity);
            }
        }
        private void OnZoneDeleted(ZoneDeletedEvent e)
        {
            if (e.Zone is not Stockpile stockpile)
                return;
            this._allStockpiles.Remove(stockpile);
            this._allStockpilesById.Remove(stockpile.ID);
            foreach (var cell in stockpile.Positions)
                this.CellsToStockpiles.Remove(cell);
        }
        private void OnZoneCreated(ZoneCreatedEvent e)
        {
            if (e.Zone is not Stockpile stockpile)
                return;
            this._allStockpiles.Add(stockpile);
            this._allStockpilesById[stockpile.ID] = stockpile;

            foreach (var cell in stockpile.Positions)
                this.CellsToStockpiles[cell] = stockpile;
        }
        public override void Tick() { }
    }
}
