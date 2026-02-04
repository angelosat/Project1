using Project1.Framework.Entities;
using Project1.Framework.StaticMaps.Components;
using Project1.Framework.WorldGen;
using Start_a_Town_;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Stockpiles
{
    public class StockpileManager : MapComponent
    {
        readonly List<Stockpile> _allStockpiles = [];
        readonly Dictionary<ZoneId, Stockpile> _allStockpilesById = [];
        public IReadOnlyList<Stockpile> Stockpiles => this._allStockpiles;
        public IEnumerable<Entity> AllItems => this._allStockpiles.SelectMany(s => s.Items);
        public IEnumerable<Entity> GetItems(ZoneId stockpileId) => stockpileId != ZoneId.Null ? this._allStockpilesById[stockpileId].Items : this.AllItems;
        public StockpileManager(MapBase map) : base(map)
        {
            map.Events.ListenTo<ZoneCreatedEvent>(OnZoneCreated);
            map.Events.ListenTo<ZoneDeletedEvent>(OnZoneDeleted);

            map.Events.ListenTo<EntityEnteredZoneEvent>(OnEntityEnteredZone);
            map.Events.ListenTo<EntityExitedZoneEvent>(OnEntityExitedZone);
        }

        private void OnEntityExitedZone(EntityExitedZoneEvent e)
        {
            if (e.Zone is Stockpile stockpile)
                stockpile.AcceptedItems.Remove(e.Entity);
        }

        private void OnEntityEnteredZone(EntityEnteredZoneEvent e)
        {
            if (e.Zone is Stockpile stockpile)
                if (stockpile.Accepts(e.Entity))
                    stockpile.AcceptedItems.Add(e.Entity);
        }
        protected internal override void ResolveReferences()
        {
            var zonemanager = this.Map.Town.ZoneManager;
            var stockpiles = zonemanager.GetZones<Stockpile>();
            foreach (var s in stockpiles)
            {
                this._allStockpiles.Add(s);
                this._allStockpilesById[s.ID] = s;
            }

            foreach (var entity in this.Map.Entities)
            {
                if (!zonemanager.CellsToZones.TryGetValue(entity.Cell.Below, out var zone))
                    continue;
                if (zone is Stockpile sp && sp.Accepts(entity))
                    sp.AcceptedItems.Add(entity);
            }
        }

        private void OnZoneDeleted(ZoneDeletedEvent e)
        {
            if (e.Zone is not Stockpile stockpile)
                return;
            this._allStockpiles.Remove(stockpile);
            this._allStockpilesById.Remove(stockpile.ID);
        }
        private void OnZoneCreated(ZoneCreatedEvent e)
        {
            if (e.Zone is not Stockpile stockpile)
                return;
            this._allStockpiles.Add(stockpile);
            this._allStockpilesById[stockpile.ID] = stockpile;
        }
        public override void Tick() { }
    }
}
