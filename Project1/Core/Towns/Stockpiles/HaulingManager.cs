using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Simulation;
using Project1.Core.Systems.Materials;
using Project1.Core.Towns.Storage;
using Project1.Core.Towns.Zones;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Stockpiles
{
    class StockpileNode
    {
        public Def Key;
        public int Quantity;
        public StockpileNode Parent;
        public Dictionary<Def, StockpileNode> Children = new();
        private List<Entity> _items;
        public bool IsLeaf => Children.Count == 0;

        public StockpileNode GetOrCreate(Def key)
        {
            if (!Children.TryGetValue(key, out var node))
            {
                node = new StockpileNode { Key = key, Parent = this };
                Children[key] = node;
            }
            return node;
        }

        public void AddItem(Entity item)
        {
            // lazy allocation: only leaves actually use this
            (_items ??= new List<Entity>()).Add(item);
        }
        public void RemoveItem(Entity item)
        {
            _items?.Remove(item);
        }
        public IReadOnlyList<Entity> Items => _items;

    }
    delegate Def GroupSelector(Entity item);

    class StockpileTracker
    {
        private readonly List<GroupSelector> _levels;
        private readonly StockpileNode _root = new();
        Dictionary<Entity, StockpileNode> _itemToLeaf = new();

        public StockpileTracker(params GroupSelector[] levels)
        {
            _levels = levels.ToList();
        }

        public void Add(Entity item)
        {
            int qty = item.StackSize;

            var node = _root;
            node.Quantity += qty;

            foreach (var level in _levels)
            {
                var key = level(item);
                node = node.GetOrCreate(key);
                node.Quantity += qty;
            }

            node.AddItem(item); // only matters at leaf
            _itemToLeaf[item] = node;
        }

        public void Remove(Entity item)
        {
            if (!_itemToLeaf.TryGetValue(item, out var leaf))
                return;

            int qty = item.StackSize;

            // walk upwards instead of recomputing path
            var node = leaf;
            while (node != null)
            {
                node.Quantity -= qty;
                node = node.Parent;
            }

            leaf.RemoveItem(item);
            _itemToLeaf.Remove(item);
        }
        public void UpdateStack(Entity item, int oldQty, int newQty)
        {
            int delta = newQty - oldQty;

            var node = _itemToLeaf[item];
            while (node != null)
            {
                node.Quantity += delta;
                node = node.Parent;
            }
        }
    }
    class StockpileTrackerRoot
    {
        Dictionary<Def, StockpileTracker_MaterialRefinement> Root = [];

        public StockpileTrackerRoot()
        {
            foreach (var def in Def.Get<MaterialRefinementDef>())
                this.Root.Add(def, new StockpileTracker_MaterialRefinement());
        }
        internal void Add(Entity item)
        {
            if (item.Profile is MaterialRefinementDef def)
                this.Root[def].Add(item);
        }
    }
    abstract class StockpileTrackerBase
    {
        internal abstract void Add(Entity item);
    }
    abstract class StockpileTrackerBranch : StockpileTrackerBase
    {
        protected abstract Def ExtractDef(Entity item);
    }
    abstract class StockpileTrackerBranch<TTracker> : StockpileTrackerBranch
      where TTracker : StockpileTrackerBranch<TTracker>
    {
        protected Dictionary<Def, StockpileTrackerBase> Root = [];
        internal override void Add(Entity item)
        {
            var def = this.ExtractDef(item);
            if (!this.Root.TryGetValue(def, out var list))
                this.Root[def] = list = this.CreateDescendant();
            list.Add(item);
        }
        protected abstract StockpileTrackerBase CreateDescendant();

    }
    abstract class StockpileTrackerLeaf : StockpileTrackerBase
    {
        protected Dictionary<Def, List<Entity>> Root = [];
        protected abstract Def ExtractDef(Entity item);
        internal override void Add(Entity item)
        {
            var def = this.ExtractDef(item);
            if (!this.Root.TryGetValue(def, out var list))
                this.Root[def] = list = [];
            list.Add(item);
        }
    }

    sealed class StockpileTracker_Ingredient : StockpileTrackerBranch<StockpileTracker_Ingredient>
    {
        protected override StockpileTrackerBranch CreateDescendant()
            => new StockpileTracker_MaterialType();

        protected override Def ExtractDef(Entity item)
            => item.Profile;
    }
    sealed class StockpileTracker_MaterialRefinement : StockpileTrackerBranch<StockpileTracker_MaterialRefinement>
    {
        protected override StockpileTrackerBase CreateDescendant()
            => new StockpileTracker_Material();
        protected override Def ExtractDef(Entity item)
            => item.PrimaryMaterial.Type;
    }
    sealed class StockpileTracker_MaterialType : StockpileTrackerBranch<StockpileTracker_MaterialType>
    {
        protected override StockpileTrackerBase CreateDescendant()
            => new StockpileTracker_Material();

        protected override Def ExtractDef(Entity item)
            => item.PrimaryMaterial;

    }
    sealed class StockpileTracker_Material : StockpileTrackerLeaf
    {
        protected override Def ExtractDef(Entity item)
            => item.PrimaryMaterial;
    }   

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

        //StockpileTrackerRoot Tracker = new StockpileTrackerRoot();
        StockpileTracker Tracker = new StockpileTracker(
            item=> (MaterialRefinementDef)item.Profile,
            item => item.PrimaryMaterial.Type,
            item => item.PrimaryMaterial);
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
                this.UnRegister(e.Entity, stockpile);
        }
        private void OnEntityEnteredZone(EntityEnteredZoneEvent e)
        {
            if (e.Zone is Stockpile stockpile)
                if (stockpile.Accepts(e.Entity))
                    this.Register(e.Entity, stockpile);
        }

        private void Register(Entity item, Stockpile stockpile)
        {
            stockpile.AcceptedItems.Add(item);
            this.Tracker.Add(item);
        }
        private void UnRegister(Entity item, Stockpile stockpile)
        {
            stockpile.AcceptedItems.Remove(item);
        }
        protected internal override void ResolveReferences()
        {
            var zonemanager = this.Map.Town.ZoneManager;
            var stockpiles = zonemanager.GetZones<Stockpile>();
            foreach (var s in stockpiles)
            {
                TryRegister(s);
            }

        }

        internal override void Scan(BlockEntity be)
        {
            this.TryRegister(be);
        }
        internal override void Scan(Entity entity)
        {
            var zonemanager = this.Map.Town.ZoneManager;

            if (!zonemanager.CellsToZones.TryGetValue(entity.Cell.Below, out var zone))
                return;
            if (zone is Stockpile sp && sp.Accepts(entity))
                this.Register(entity, sp);
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

    //public class StockpileManager : MapComponent
    //{
    //    readonly List<Stockpile> _allStockpiles = [];
    //    readonly Dictionary<ZoneId, Stockpile> _allStockpilesById = [];
    //    public IReadOnlyList<Stockpile> Stockpiles => this._allStockpiles;
    //    public IEnumerable<Entity> AllItems => this._allStockpiles.SelectMany(s => s.Items);
    //    public IEnumerable<Entity> GetItems(ZoneId stockpileId) => stockpileId != ZoneId.Null ? this._allStockpilesById[stockpileId].Items : this.AllItems;

    //    Dictionary<IntVec3, BlockInventoryComp> BlockEntities = [];
    //    public StockpileManager(MapBase map) : base(map)
    //    {
    //        map.Events.ListenTo<ZoneCreatedEvent>(OnZoneCreated);
    //        map.Events.ListenTo<ZoneDeletedEvent>(OnZoneDeleted);

    //        map.Events.ListenTo<EntityEnteredZoneEvent>(OnEntityEnteredZone);
    //        map.Events.ListenTo<EntityExitedZoneEvent>(OnEntityExitedZone);
    //    }
    //    private void OnEntityExitedZone(EntityExitedZoneEvent e)
    //    {
    //        if (e.Zone is Stockpile stockpile)
    //            stockpile.AcceptedItems.Remove(e.Entity);
    //    }
    //    private void OnEntityEnteredZone(EntityEnteredZoneEvent e)
    //    {
    //        if (e.Zone is Stockpile stockpile)
    //            if (stockpile.Accepts(e.Entity))
    //                stockpile.AcceptedItems.Add(e.Entity);
    //    }
    //    protected internal override void ResolveReferences()
    //    {
    //        var zonemanager = this.Map.Town.ZoneManager;
    //        var stockpiles = zonemanager.GetZones<Stockpile>();
    //        foreach (var s in stockpiles)
    //        {
    //            this._allStockpiles.Add(s);
    //            this._allStockpilesById[s.ID] = s;
    //        }

    //        //foreach (var entity in this.Map.Entities)
    //        //{
    //        //    if (!zonemanager.CellsToZones.TryGetValue(entity.Cell.Below, out var zone))
    //        //        continue;
    //        //    if (zone is Stockpile sp && sp.Accepts(entity))
    //        //        sp.AcceptedItems.Add(entity);
    //        //}
    //    }
    //    internal override void Scan(Entity entity)
    //    {
    //        var zonemanager = this.Map.Town.ZoneManager;

    //        if (!zonemanager.CellsToZones.TryGetValue(entity.Cell.Below, out var zone))
    //            return;
    //        if (zone is Stockpile sp && sp.Accepts(entity))
    //            sp.AcceptedItems.Add(entity);
    //    }
    //    private void OnZoneDeleted(ZoneDeletedEvent e)
    //    {
    //        if (e.Zone is not Stockpile stockpile)
    //            return;
    //        this._allStockpiles.Remove(stockpile);
    //        this._allStockpilesById.Remove(stockpile.ID);
    //    }
    //    private void OnZoneCreated(ZoneCreatedEvent e)
    //    {
    //        if (e.Zone is not Stockpile stockpile)
    //            return;
    //        this._allStockpiles.Add(stockpile);
    //        this._allStockpilesById[stockpile.ID] = stockpile;
    //    }
    //    public override void Tick() { }
    //}
}
