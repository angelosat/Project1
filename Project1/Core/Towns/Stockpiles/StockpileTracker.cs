using Project1.Core.Entities;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Stockpiles;

class StockpileNode : ICollapsibleNode<Def>
{
    public Def Key;
    public int Quantity;
    public StockpileNode Parent;
    Dictionary<Def, StockpileNode> _children = [];
    internal IReadOnlyDictionary<Def, StockpileNode> Children => this._children;
    public event Action<ICollapsibleNode<Def>> ChildAdded;
    public event Action<ICollapsibleNode<Def>> ChildRemoved;
    public event Action<ICollapsibleNode<Def>> Updated;
    private List<Entity> _items;
    public bool IsLeaf => this._children.Count == 0;
    public string Label { get => this.Key?.LabelReadable ?? field; set => field = value; }

    internal StockpileNode GetOrCreate(Def key, out bool created)
    {
        created = false;
        if (!this._children.TryGetValue(key, out var node))
        {
            node = new StockpileNode { Key = key, Parent = this };
            this._children[key] = node;
            this.ChildAdded?.Invoke(node);
            created = true;
        }
        return node;
    }
    internal void RemoveNode(Def def)
    {
        this._children.Remove(def, out var node);
        this.ChildRemoved?.Invoke(node);
    }
    internal void AddItem(Entity item)
    {
        // lazy allocation: only leaves actually use this
        (this._items ??= []).Add(item);
    }
    internal void RemoveItem(Entity item)
    {
        this._items?.Remove(item);
    }

    public Control GetControl()
        => new LabelNew(() => $"{this.Label} {this.Quantity}");

    public IReadOnlyList<Entity> Items => _items;

    ICollapsibleNode<Def> ICollapsibleNode<Def>.Parent => Parent;

    IEnumerable<ICollapsibleNode<Def>> ICollapsibleNode<Def>.Children => this._children.Values;
}
delegate Def GroupSelector(Entity item);

sealed class StockpileTrackerManager
{
    public StockpileTrackerManager(params (string label, Type defType, GroupSelector[] selectors)[] args)
    {
        foreach (var arg in args)
        {
            this.Trackers.Add(arg.defType, new(arg.label, arg.selectors));
        }
    }
    internal Dictionary<Type, StockpileTracker> Trackers = [];
    internal Control GetControl()
    {
        var list = new ListCollapsible<Def>();
        var nodes = this.Trackers.Values;
        list.Build(nodes.Select(n => n.Root));
        return list;
    }
    internal void Add(Entity item)
    {
        this.Trackers[item.Profile.GetType()].Add(item);
    }
    internal void Remove(Entity item)
    {
        this.Trackers[item.Profile.GetType()].Remove(item);
    }
}

class StockpileTracker
{
    private readonly List<GroupSelector> _levels;
    private readonly StockpileNode _root = new();
    internal StockpileNode Root => this._root;
    readonly Dictionary<Entity, StockpileNode> _itemToLeaf = [];

    event Action<(IEnumerable<StockpileNode> added, IEnumerable<StockpileNode> removed)> NodesUpdated;
    event Action<StockpileNode> NodeAdded;

    public StockpileTracker(string label, params GroupSelector[] levels)
    {
        this._root.Label = label;
        this._levels = [.. levels];
    }

    public void Add(Entity item)
    {
        int qty = item.StackSize;

        var node = _root;
        node.Quantity += qty;

        foreach (var level in _levels)
        {
            var key = level(item);
            node = node.GetOrCreate(key, out var created);
            if (created)
                this.NodesUpdated?.Invoke(([node], []));
            node.Quantity += qty;
        }

        node.AddItem(item); // only matters at leaf
        this._itemToLeaf[item] = node;
    }
    public void Remove(Entity item)
    {
        if (!_itemToLeaf.TryGetValue(item, out var leaf))
            return;

        int qty = item.StackSize;

        // remove from leaf storage
        leaf.RemoveItem(item);
        this._itemToLeaf.Remove(item);

        var node = leaf;

        while (node != null)
        {
            node.Quantity -= qty;

            var parent = node.Parent;

            // stop at root (never remove root)
            if (parent == null)
                break;

            // prune if empty
            if (node.Quantity == 0 && node.Children.Count == 0)
            {
                parent.RemoveNode(node.Key);
                this.NodesUpdated?.Invoke(([], [node]));
            }

            node = parent;
        }
    }

    internal Control GetControl()
    {
        var list = new ListCollapsible<Def>();
        list.Build([this._root]);
        return list;
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

//class StockpileTrackerRoot
//{
//    Dictionary<Def, StockpileTracker_MaterialRefinement> Root = [];

//    public StockpileTrackerRoot()
//    {
//        foreach (var def in Def.Get<MaterialRefinementDef>())
//            this.Root.Add(def, new StockpileTracker_MaterialRefinement());
//    }
//    internal void Add(Entity item)
//    {
//        if (item.Profile is MaterialRefinementDef def)
//            this.Root[def].Add(item);
//    }
//}
//abstract class StockpileTrackerBase
//{
//    internal abstract void Add(Entity item);
//}
//abstract class StockpileTrackerBranch : StockpileTrackerBase
//{
//    protected abstract Def ExtractDef(Entity item);
//}
//abstract class StockpileTrackerBranch<TTracker> : StockpileTrackerBranch
//  where TTracker : StockpileTrackerBranch<TTracker>
//{
//    protected Dictionary<Def, StockpileTrackerBase> Root = [];
//    internal override void Add(Entity item)
//    {
//        var def = this.ExtractDef(item);
//        if (!this.Root.TryGetValue(def, out var list))
//            this.Root[def] = list = this.CreateDescendant();
//        list.Add(item);
//    }
//    protected abstract StockpileTrackerBase CreateDescendant();

//}
//abstract class StockpileTrackerLeaf : StockpileTrackerBase
//{
//    protected Dictionary<Def, List<Entity>> Root = [];
//    protected abstract Def ExtractDef(Entity item);
//    internal override void Add(Entity item)
//    {
//        var def = this.ExtractDef(item);
//        if (!this.Root.TryGetValue(def, out var list))
//            this.Root[def] = list = [];
//        list.Add(item);
//    }
//}

//sealed class StockpileTracker_Ingredient : StockpileTrackerBranch<StockpileTracker_Ingredient>
//{
//    protected override StockpileTrackerBranch CreateDescendant()
//        => new StockpileTracker_MaterialType();

//    protected override Def ExtractDef(Entity item)
//        => item.Profile;
//}
//sealed class StockpileTracker_MaterialRefinement : StockpileTrackerBranch<StockpileTracker_MaterialRefinement>
//{
//    protected override StockpileTrackerBase CreateDescendant()
//        => new StockpileTracker_Material();
//    protected override Def ExtractDef(Entity item)
//        => item.PrimaryMaterial.Type;
//}
//sealed class StockpileTracker_MaterialType : StockpileTrackerBranch<StockpileTracker_MaterialType>
//{
//    protected override StockpileTrackerBase CreateDescendant()
//        => new StockpileTracker_Material();

//    protected override Def ExtractDef(Entity item)
//        => item.PrimaryMaterial;

//}
//sealed class StockpileTracker_Material : StockpileTrackerLeaf
//{
//    protected override Def ExtractDef(Entity item)
//        => item.PrimaryMaterial;
//}   
