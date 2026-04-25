using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Systems.Ownership;
using Project1.Core.Towns.Stockpiles;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Services.Shops;
public sealed class TownComp_Shops : TownComp
{
    internal readonly ChangeNotifier Notifier = new();

    Dictionary<EntityRefId, PriceTag> _itemsForSale = [];
    readonly Dictionary<Def, HashSet<EntityRefId>> _itemsForSaleByProfile = [];

    readonly Dictionary<EntityRefId, ServiceRequest_Shop> _transactionsRequests = [];
    readonly Dictionary<EntityRefId, ServiceRequest_Shop> _transactionsBySeller = [];
    readonly Dictionary<EntityRefId, ServiceRequest_Shop> _transactionsByBuyer = [];
    readonly Dictionary<EntityRefId, ServiceRequest_Shop> _transactionsActive = [];
    readonly Dictionary<EntityRefId, ServiceRequest_Shop> _transactionsByItem = [];

    readonly List<ServiceRequest_Shop> _transactionsAll = [];
    internal event Action<(IEnumerable<Entity> added, IEnumerable<Entity> removed)> ItemsForSaleToggled;
    readonly ObservableHashSet<Workplace> Shopss = new();

    readonly Dictionary<IntVec3, BlockShelfComp> Shelves = [];
    public IEnumerable<IntVec3> EmptyShelves
        => this.Shelves.Keys.Where(sh => this.Map.IsCellEmpty(sh.Above));


    readonly Dictionary<TownServiceDef, TownServiceWorker> _servicesByDef = [];
    readonly Dictionary<Type, TownServiceWorker> _servicesByType = [];
    //readonly HashSet<IntVec3> ServicePoints = [];


    internal int CurrentShopID = 1;

    Dictionary<int, Workplace> Shops = [];

    static TownComp_Shops()
    {
        Tavern.Init();
    }

    public TownComp_Shops(Town town) : base(town)
    {
        //town.Map.Events.ListenTo<BlocksChangedEvent>(HandleBlocksChanged);
        town.Map.Events.ListenTo<StockpileUpdatedEvent>(HandleStockpileUpdated);
        town.Map.Events.ListenTo<EntityDespawnedEvent>(HandleEntityDespawned);
        town.Map.World.Events.ListenTo<EntityDisposedEvent>(HandleEntityDisposed);
        town.Map.World.Events.ListenTo<ItemOwnerChangedEvent>(HandleItemOwnerChanged);
        town.Map.Events.ListenTo<BlockEntityAddedEvent>(HandleBlockEntityAdded);
        town.Map.Events.ListenTo<BlockEntityRemovedEvent>(HandleBlockEntityRemoved);
    }

    private void HandleBlockEntityAdded(BlockEntityAddedEvent e)
    {
        if (!e.Entity.TryGetComp<BlockShelfComp>(out var comp))
            return;
        this.Register(comp);
    }

    private void Register(BlockShelfComp comp)
    {
        this.Shelves.Add(comp.Parent.OriginGlobal, comp); // or cellsoccupied?
    }

    private void HandleBlockEntityRemoved(BlockEntityRemovedEvent e)
    {
        if (!e.Entity.TryGetComp<BlockShelfComp>(out var comp))
            return;
        this.UnRegister(comp);
    }
    private void UnRegister(BlockShelfComp comp)
    {
        this.Shelves.Remove(comp.Parent.OriginGlobal); // or cellsoccupied?
    }
    private void HandleItemOwnerChanged(ItemOwnerChangedEvent e)
    {
        if (this._itemsForSale.ContainsKey(e.Item.RefId))
            this.ToggleForSale(e.Item);
    }

    // todo: cache comps to avoid lookups
    internal bool CanShelfAccept(IntVec3 shelf, Entity product)
        => this.Shelves[shelf].Accepts(product);
    private void HandleEntityDisposed(EntityDisposedEvent e)
    {
        if (this._itemsForSale.ContainsKey(e.Entity.RefId))
            this.ToggleForSale(e.Entity);
    }

    private void HandleEntityDespawned(EntityDespawnedEvent e)
    {
        if (e.Entity is Actor actor)
            this._shoppingListsByActor.Remove(actor.RefId);
    }

    private void HandleStockpileUpdated(StockpileUpdatedEvent e)
    {
        if (!e.Stockpile.ForSale)
            return;
        foreach(var item in e.Stockpile.Items)
            foreach(var actorid in this._shoppingListsByActor.Keys)
                this.Map.World.Get<Actor>(actorid).AI.State.ItemPreferences.TryEnqueue(item);
    }

    //private void HandleBlocksChanged(BlocksChangedEvent e)
    //{
    //    foreach(var pos in e.Changes)
    //    {
    //        if (pos.Block == BlockDefOf.ShopCounter.Block)
    //            this.ServicePoints.Add(pos.Global);
    //        else
    //            this.ServicePoints.Remove(pos.Global);
    //    }
    //}

    public T GetService<T>() where T : TownServiceWorker => (T)this._servicesByType[typeof(T)];
    public override string Name => "Shops";
    public Shop CreateShop()
    {
        var newshop = new Shop(this, this.GetNextShopID());
        this.AddShop(newshop);
        return newshop;
    }
    public Shop CreateShop(int id)
    {
        var newshop = new Shop(this, id);
        this.AddShop(newshop);
        return newshop;
    }
    public void AddShop(Workplace shop)
    {
        this.Shopss.Add(shop);
        this.Shops.Add(shop.ID, shop);
    }

    public T FindShop<T>(Stockpile stockpile) where T : Workplace
    {
        return this.Shopss.FirstOrDefault(sh => sh.HasStockpile(stockpile.ID)) as T;
    }

    public int GetNextShopID()
    {
        return this.CurrentShopID++;
    }

    public Workplace GetShop(int shopid)
    {
        if (shopid < 0)
            return null;
        return this.Shops[shopid];
    }

    public Workplace GetShop(Actor worker)
    {
        return this.Shopss.FirstOrDefault(s => s.HasWorker(worker));
    }

    public T GetShop<T>(Actor worker) where T : Workplace
    {
        return this.Shopss.FirstOrDefault(s => s.HasWorker(worker)) as T;
    }

    public IEnumerable<Workplace> GetShops()
    {
        foreach (var shop in this.Shops.Values)
            yield return shop;
    }
    readonly Dictionary<EntityRefId, ShoppingList> _shoppingListsByActor = [];
    internal ShoppingList GetShoppingListEmpty(Actor buyer)
    {
        if (!this._shoppingListsByActor.TryGetValue(buyer.RefId, out var list))
            this._shoppingListsByActor[buyer.RefId] = list = new(buyer, []);
        return list;
    }
    internal ShoppingList GetShoppingListPopulated(Actor buyer)
    {
        if (!this._shoppingListsByActor.TryGetValue(buyer.RefId, out var list))
            //this._shoppingListsByActor[buyer.RefId] = list = new(buyer, [.. this.GetStockpileItemsForSale()]);
            this._shoppingListsByActor[buyer.RefId] = list = new(buyer, [.. this.GetItemsMarkedForSale()]);
        return list;
    }


    internal IEnumerable<(Entity entity, PriceTag price)> GetPriceList()
    {
        foreach (var pricetag in this._itemsForSale)
            yield return (this.World.Get(pricetag.Key), pricetag.Value);
    }
    internal int CountForSale(Def d)
       => this._itemsForSaleByProfile.TryGetValue(d, out var set) ? set.Count : 0;

    internal void ToggleForSale(Entity item)
    {
        var forsale = false;
        if (!this._itemsForSale.Remove(item.RefId))
        {
            forsale = true;
            this._itemsForSale.Add(item.RefId, new(item.RefId, item.GetValueTotal()));
            if (!this._itemsForSaleByProfile.TryGetValue(item.Profile, out var set))
                this._itemsForSaleByProfile[item.Profile] = set = [];
            set.Add(item.RefId);
            this.ItemsForSaleToggled?.Invoke(([item], []));
        }
        else
        {
            var set = this._itemsForSaleByProfile[item.Profile];
            set.Remove(item.RefId);
            if (set.Count == 0)
                this._itemsForSaleByProfile.Remove(item.Profile);

            if (this._transactionsByItem.TryGetValue(item.RefId, out var req))
                req.MarkFailed();

            this.ItemsForSaleToggled?.Invoke(([], [item]));
        }
        this.Notifier.Notify();
        this.Map.Events.Post(new ItemToggledForSaleEvent(item, forsale));
    }
   
    internal bool IsForSale(Entity item)
        => this._itemsForSale.ContainsKey(item.RefId);
    internal int? GetPrice(EntityRefId itemId)
        => this._itemsForSale.TryGetValue(itemId, out var tag) ? tag.Price : null;
    internal int? GetPrice(Entity item)
    => this._itemsForSale.TryGetValue(item.RefId, out var tag) ? tag.Price : null;
    internal bool TryBegin(Actor actor, Entity item, int price, IntVec3 servicePoint, out ServiceRequest_Shop req)
    {
        req = new ServiceRequest_Shop(actor, item, price, servicePoint);
        AddInt(req);
        this.Town.OpenTransactions.Add(actor.RefId, req);
        actor.Map.Events.Post(new TransactionStartedEvent(actor.Map, req));
        return true;
        // TODO made it a bool return in case i have some conditions that can make it fail in the future
    }

    private void AddInt(ServiceRequest_Shop req)
    {
        this._transactionsAll.Add(req);
        this._transactionsRequests.Add(req.Customer, req);
        this._transactionsByBuyer.Add(req.Customer, req);
        if (req.Vendor != EntityRefId.Null)
            this._transactionsBySeller.Add(req.Vendor, req);
        this._transactionsByItem.Add(req.Item, req);
        this.Town.ServiceRequests.Register(req);
    }

    internal bool TryGetTransaction(EntityRefId buyer, out ServiceRequest_Shop transaction)
      => this._transactionsByBuyer.TryGetValue(buyer, out transaction);
    internal bool TryGetTransaction(Actor buyer, out ServiceRequest_Shop transaction)
        => this._transactionsByBuyer.TryGetValue(buyer.RefId, out transaction);
    internal ServiceRequest_Shop GetTransaction(Actor buyer)
        => this._transactionsByBuyer.TryGetValue(buyer.RefId, out var t) ? t : null;
    
    internal bool TryGetTransactionBySeller(Actor seller, out ServiceRequest_Shop transaction)
        => this._transactionsBySeller.TryGetValue(seller.RefId, out transaction);
    internal bool TryGetTransactionByItem(Entity item, out ServiceRequest_Shop transaction)
        => this._transactionsByItem.TryGetValue(item.RefId, out transaction);
    internal ServiceRequest_Shop GetTransactionBySeller(Actor seller)
           => this._transactionsBySeller.TryGetValue(seller.RefId, out var t) ? t : null;
    internal IEnumerable<ServiceRequest_Shop> PendingTransactions => this._transactionsRequests.Values;
    internal void AssignSeller(ServiceRequest_Shop transaction, Actor seller)
    {
        if (transaction.Vendor != EntityRefId.Null)
            throw new Exception();
        this.Town.ServiceRequests.AssignVendor(transaction, seller);
        transaction.RefreshTimer();
        this._transactionsBySeller.Add(seller.RefId, transaction);
        this._transactionsRequests.Remove(transaction.Customer);
    }
    internal override void Tick()
    {
        foreach(var transaction in this._transactionsAll.ToArray())
        {
            if (transaction.IsFailed || transaction.IsSucceeded)
            {
                this._transactionsAll.Remove(transaction);
                this._transactionsActive.Remove(transaction.Customer);
                this._transactionsRequests.Remove(transaction.Customer);
                this._transactionsByBuyer.Remove(transaction.Customer);
                this._transactionsByItem.Remove(transaction.Item);
                this.Town.OpenTransactions.Remove(transaction.Customer);
                if (transaction.Vendor != EntityRefId.Null)
                    this._transactionsBySeller.Remove(transaction.Vendor);
                this.Town.ServiceRequests.Remove(transaction.Id);
                this.Map.Events.Post(new TownServiceCompleteEvent(this.Map, transaction));
            }
        }
    }
    protected override void SaveExtra(SaveTag tag)
    {
        tag.Add(this.CurrentShopID.Save("ShopIDSequence"));
        this.Shopss.SaveAbstract(tag, "Shops");
        tag.Save("PriceList", this._itemsForSale.Values);
    }

    public override void Load(SaveTag tag)
    {
        tag.TryGetTagValue("ShopIDSequence", ref this.CurrentShopID);
        this.Shopss.LoadAbstract(tag, "Shops", this);
        this.Shops = this.Shopss.ToDictionary(i => i.ID, i => i);
        if (tag.TryLoadList<PriceTag>("PriceList", out var prices))
            this._itemsForSale = prices.ToDictionary(a => a.Item);
    }

    public override void Write(IDataWriter w)
    {
        w.Write(this.CurrentShopID);
        this.Shopss.WriteAbstract(w);
        w.Write(this._itemsForSale.Values);
    }

    public override void Read(IDataReader r)
    {
        this.CurrentShopID = r.ReadInt32();
        this.Shopss.ReadListAbstract(r, this);
        this.Shops = this.Shopss.ToDictionary(i => i.ID, i => i);
        this._itemsForSale = r.ReadList<PriceTag>().ToDictionary(a => a.Item);
    }

    public void RemoveShop(int shopid)
    {
        var shop = this.Shops[shopid];
        this.Shopss.Remove(shop);
        this.Shops.Remove(shopid);
    }

    public bool ShopExists(Workplace shop)
    {
        return this.Shops.ContainsKey(shop.ID);
    }

   
    
    internal override void OnBlocksChanged(IEnumerable<IntVec3> positions)
    {
        foreach (var wp in this.Shopss)
            wp.OnBlocksChanged(positions);
    }

    internal override IEnumerable<(Func<string>, Action)> OnQuickMenuCreated()
    {
        //yield return (() => "Shops", () => UIManager.ToggleSingleton<WorkplacesGui>("Shops"));
        yield return (() => "Shops", () => UIManager.ToggleSingleton<Gui_PriceList>("Shops"));
    }

    internal override void ResolveReferences()
    {
        foreach (var wp in this.Shopss)
            wp.ResolveReferences();

        //foreach(var cell in this.Map.GetAllCellsWithIndex())
        //{
        //    if (cell.cell.Block == BlockDefOf.ShopCounter.Block)
        //        this.ServicePoints.Add(cell.id.Local.ToGlobal(cell.chunk));
        //}

        foreach (var req in this.Town.ServiceRequests.GetAllRequests<ServiceRequest_Shop>())
            this.AddInt(req);
    }

    

    internal void ToggleWorker(Actor a, Workplace shop)
    {
        PacketsWorkplaces.SendPlayerAssignWorkerToShop(a.Net, a.Net.GetPlayer(), shop.Map, a, shop);
    }
   

    internal void MarkPaid(Actor buyer, Entity money)
    {
        var req = this._transactionsByBuyer[buyer.RefId];
        req.AllocateMoney(money);
    }
    internal void RingUp(Actor seller, Entity item)
    {
        var req = this._transactionsBySeller[seller.RefId];
        if (item.RefId != req.Item)
            throw new InvalidOperationException();
        req.MarkVendorWaitingPayment();
    }
    internal void FinishTransaction(Actor buyer)
    {
        var req = this._transactionsByBuyer[buyer.RefId];
        var shoppinglist = this._shoppingListsByActor[buyer.RefId];
        shoppinglist.MarkFulfilled();
        req.MarkSucceeded();
    }
    internal void MarkPaidFor(Actor seller)
    {
        var req = this._transactionsBySeller[seller.RefId];
        req.MarkPaidFor();
    }

    internal IEnumerable<Entity> GetStockpileItemsForSale()
        => this.Town.Map.Hauling.Stockpiles
        .Where(s => s.ForSale)
        .SelectMany(s => s.Items);
    internal IEnumerable<Entity> GetItemsMarkedForSale()
        //=> this._itemsForSale.Keys.Select(id => this.World.Get(id));
        => this.World.Get(this._itemsForSale.Keys);
    internal IEnumerable<(Entity item, IEnumerable<IntVec3> shelves)> GetRestockingOptions()
    {
        foreach(var item in this.GetItemsMarkedForSale())
            yield return (item, this.GetShelvesForItem(item));
    }
    internal IEnumerable<Entity> GetItemsForShelf(IntVec3 shelf)
        => this.GetItemsMarkedForSale().Where(this.Shelves[shelf].Accepts);
    internal IEnumerable<IntVec3> GetShelvesForItem(Entity item)
    {
        foreach (var (pos, comp) in this.Shelves)
            if (comp.Accepts(item))
                yield return pos;
    }
    internal bool IsItemAtCorrectShelf(Entity item)
        => this.Shelves.TryGetValue(item.Cell.Below, out var shelf) && shelf.Accepts(item);
    internal IEnumerable<Entity> GetItemsNeedingRestock()
    {
        foreach(var item in this.GetItemsMarkedForSale())
        {
            if (this.IsItemAtCorrectShelf(item))
                continue;
            yield return item;
        }
    }

    internal override bool IsClaimedBySystem(Entity item)
    {
        if (item.IsForSale && this.IsItemAtCorrectShelf(item))
            return true;
        if (this._transactionsByItem.ContainsKey(item.RefId))
            return true;
        //foreach(var t in this._transactionsAll)
        //{
        //    if (t.Item != item.RefId)
        //        continue;
        //    if (item.Map != this.Map)
        //        continue;
        //    if (item.Cell != t.Counter.Value.Above)
        //        continue;
        //    return true;
        //}
        return false;
    }

    internal override void Scan(BlockEntity entity)
    {
        if (entity.TryGetComp<BlockShelfComp>(out var shelf))
            this.Register(shelf);
    }

   
}
