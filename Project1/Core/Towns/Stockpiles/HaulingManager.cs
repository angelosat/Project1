using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Simulation;
using Project1.Core.Towns.Storage;
using Project1.Core.Towns.Zones;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Stockpiles
{
    public sealed class HaulingManager : MapComponent
    {
        readonly List<Stockpile> _allStockpiles = [];
        readonly Dictionary<ZoneId, Stockpile> _allStockpilesById = [];
        public IReadOnlyList<Stockpile> Stockpiles => this._allStockpiles;
        public IEnumerable<Entity> AllItems => this._allStockpiles.SelectMany(s => s.Items);
        public IEnumerable<Entity> GetItems(ZoneId stockpileId) => stockpileId != ZoneId.Null ? this._allStockpilesById[stockpileId].Items : this.AllItems;

        readonly Dictionary<IntVec3, BlockInventoryHaulingTarget> BlockEntities = [];
        readonly Dictionary<ZoneId, StockpileHaulingTarget> StockpileTargets = [];

        readonly HashSet<IHaulingTarget> _allTargets = [];
        public IReadOnlySet<IHaulingTarget> AllTargets => this._allTargets;
        public IEnumerable<Entity> InventoryItems => this.BlockEntities.Values.SelectMany(comp => comp.Items);
        public HaulingManager(MapBase map) : base(map)
        {
            map.Events.ListenTo<ZoneCreatedEvent>(OnZoneCreated);
            map.Events.ListenTo<ZoneDeletedEvent>(OnZoneDeleted);

            map.Events.ListenTo<EntityEnteredZoneEvent>(OnEntityEnteredZone);
            map.Events.ListenTo<EntityExitedZoneEvent>(OnEntityExitedZone);

            map.Events.ListenTo<BlockEntityAddedEvent>(OnBlockEntityAdded);
            map.Events.ListenTo<BlockEntityRemovedEvent>(OnBlockEntityRemoved);
        }

        private void OnBlockEntityRemoved(BlockEntityRemovedEvent e)
        {
            if (this.BlockEntities.TryGetValue(e.Entity.OriginGlobal, out var comp))
                this._allTargets.Remove(comp);
            this.BlockEntities.Remove(e.Entity.OriginGlobal);
        }

        private void OnBlockEntityAdded(BlockEntityAddedEvent e)
        {
            this.TryRegister(e.Entity);
        }

        private bool TryRegister(BlockEntity be)
        {
            if (!be.TryGetComp<BlockInventoryComp>(out var comp))
                return false;
            var tar = new BlockInventoryHaulingTarget(comp);
            this.BlockEntities.Add(be.OriginGlobal, tar);
            this._allTargets.Add(tar);
            return true;
        }
        private void TryRegister(Stockpile s)
        {
            this._allStockpiles.Add(s);
            this._allStockpilesById[s.ID] = s;
            var tar = new StockpileHaulingTarget(s);
            this._allTargets.Add(tar);
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
                TryRegister(s);
            }

            foreach (var entity in this.Map.Entities)
            {
                if (!zonemanager.CellsToZones.TryGetValue(entity.Cell.Below, out var zone))
                    continue;
                if (zone is Stockpile sp && sp.Accepts(entity))
                    sp.AcceptedItems.Add(entity);
            }

            foreach(var be in this.Map.BlockEntities)
                this.TryRegister(be);
        }

       

        private void OnZoneDeleted(ZoneDeletedEvent e)
        {
            if (e.Zone is not Stockpile stockpile)
                return;
            this._allStockpiles.Remove(stockpile);
            this._allStockpilesById.Remove(stockpile.ID);
            this.StockpileTargets.Remove(stockpile.ID);
        }
        private void OnZoneCreated(ZoneCreatedEvent e)
        {
            if (e.Zone is not Stockpile stockpile)
                return;
            this._allStockpiles.Add(stockpile);
            this._allStockpilesById[stockpile.ID] = stockpile;
            this.StockpileTargets.Add(stockpile.ID, new(stockpile));
        }
        public override void Tick() { }
    }

    public class StockpileManager : MapComponent
    {
        readonly List<Stockpile> _allStockpiles = [];
        readonly Dictionary<ZoneId, Stockpile> _allStockpilesById = [];
        public IReadOnlyList<Stockpile> Stockpiles => this._allStockpiles;
        public IEnumerable<Entity> AllItems => this._allStockpiles.SelectMany(s => s.Items);
        public IEnumerable<Entity> GetItems(ZoneId stockpileId) => stockpileId != ZoneId.Null ? this._allStockpilesById[stockpileId].Items : this.AllItems;

        Dictionary<IntVec3, BlockInventoryComp> BlockEntities = [];
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
