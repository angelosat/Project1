using Project1.Core.Blocks;
using Project1.Core.Components;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Towns.Stockpiles;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Services.Shops;

internal record struct TransactionStartedEvent(MapBase Map, ServiceRequest_Shop Transaction) : IEventPayload { }
[EnsureStaticCtorCall]
internal static class PacketsShops
{
    internal readonly static PacketId 
        _pShopCreated = Registry.PacketHandlers.Register(ReceiveCreateShop), 
        _pShopDeleted, 
        _pPlayerShopCreated = Registry.PacketHandlers.Register(ReceivePlayerCreateShop),
        _pPlayerShopDeleted;

    static PacketsShops()
    {
        Registry.PlayerInputEventHooks.Register<PlayerCreateShopEvent>(HandlePlayerCreateShop);
    }

    private static void HandlePlayerCreateShop(PlayerCreateShopEvent e)
    {
        if (Ingame.Net.IsServer)
            Ingame.MainViewportMap.Town.Shops.CreateShop();
        else
            SendPlayerCreateShop(Client.Instance, e.MapId);
    }

    private static void SendPlayerCreateShop(Client client, MapId mapid)
    {
        client.BeginPacketImmediate(_pPlayerShopCreated)
            .Write(client.PlayerData.ID)
            .Write(mapid)
            ;
    }
    private static void ReceivePlayerCreateShop(NetEndpoint endpoint, Packet packet)
    {
        var server = endpoint as Server;
        var r = packet.PacketReader;
        var playerid = r.ReadInt32();
        var mapid = r.ReadMapId();
        var map = endpoint.World.Get(mapid);
        var shop = map.Town.Shops.CreateShop();
        SendCreateShop(server, shop);
    }
    private static void SendCreateShop(Server server, Shop shop)
    {
        server.BeginPacketImmediate(_pShopCreated)
            .Write(shop.Map.ID)
            .Write(shop.ID);
    }
    private static void ReceiveCreateShop(NetEndpoint endpoint, Packet packet)
    {
        var client = endpoint as Client;
        var r = packet.PacketReader;
        var mapid = (MapId)r.ReadInt32();
        var map = client.World.Get(mapid);
        var shopid = r.ReadInt32();
        map.Town.Shops.CreateShop(shopid);
    }
}

internal record struct PlayerDeleteShopEvent(Workplace Workplace) : IEventPayload { }
internal record struct PlayerCreateShopEvent(MapId MapId) : IEventPayload { }
class WorkplacesGui : GroupBox
{
    public WorkplacesGui()
    {
        var boxList = new GroupBox();
        var town = Ingame.MainViewportMap.Town;
        var shopUI = new Lazy<(Control control, Action<Workplace> refresh)>(Workplace.CreateUI);
        var win = new Lazy<Window>(() => shopUI.Value.control.ToWindow("Shop"));

        var shoplist = new TableCompact<Workplace>()
            .AddColumn(new(), "name", 200, sh => new Label(() => sh.Name, () =>
            {
                shopUI.Value.refresh(sh);
            }), 0)
            .AddColumn(new(), "delete", Icon.Cross.SourceRect.Width,
                w => IconButton.CreateSmall(Icon.Cross,
                    () => MessageBox.CreateDialogue("Warning!", $"{w.Name} will be deleted. Are you sure?",
                        //() => Packets.SendPlayerDeleteShop(this.Town.Net, this.Town.Net.GetPlayer(), w.ID)
                        () => town.Map.Events.Post(new PlayerDeleteShopEvent(w))
                        )));

        var shoplistcontainer = shoplist.MakeScrollable(-1, 200);

        var btnNew = new Button("New", () => town.Map.Events.Post(new PlayerCreateShopEvent()));
        boxList.AddControlsVertically(shoplistcontainer, btnNew);
        this.AddControlsHorizontally(boxList);
    }
}
public partial class TownComp_Shops : TownComp
{
    public ChangeNotifier Notifications = new();
    const int UIListWidth = 250;

    readonly ObservableHashSet<Workplace> Shopss = new();

    readonly Dictionary<TownServiceDef, TownServiceWorker> _servicesByDef = [];
    readonly Dictionary<Type, TownServiceWorker> _servicesByType = [];
    readonly HashSet<IntVec3> ServicePoints = [];
    public IReadOnlySet<IntVec3> GetServicePoints()
        => this.ServicePoints;
    public bool HasServicePoints => this.ServicePoints.Count > 0;
    internal int CurrentShopID = 1;

    Dictionary<int, Workplace> Shops = [];

    static TownComp_Shops()
    {
        Tavern.Init();
    }

    public TownComp_Shops(Town town) : base(town)
    {
        town.Map.Events.ListenTo<BlocksChangedEvent>(HandleBlocksChanged);
        town.Map.Events.ListenTo<StockpileUpdatedEvent>(HandleStockpileUpdated);
        town.Map.Events.ListenTo<EntityDespawnedEvent>(HandleEntityDespawned);
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

    private void HandleBlocksChanged(BlocksChangedEvent e)
    {
        foreach(var pos in e.Changes)
        {
            if (pos.Block == BlockDefOf.ShopCounter.Block)
                this.ServicePoints.Add(pos.Global);
            else
                this.ServicePoints.Remove(pos.Global);
        }
    }

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
        this.Notifications.Notify();
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
            this._shoppingListsByActor[buyer.RefId] = list = new(buyer, [.. this.GetItemsForSale()]);
        return list;
    }

    readonly Dictionary<EntityRefId, ServiceRequest_Shop> _transactionsRequests = [];
    readonly Dictionary<EntityRefId, ServiceRequest_Shop> _transactionsBySeller = [];
    readonly Dictionary<EntityRefId, ServiceRequest_Shop> _transactionsByBuyer = [];
    readonly Dictionary<EntityRefId, ServiceRequest_Shop> _transactionsActive = [];
    readonly Dictionary<EntityRefId, ServiceRequest_Shop> _transactionsByItem = [];
    readonly List<ServiceRequest_Shop> _transactionsAll = [];
    internal bool TryBeginTransaction(Actor actor, Entity item, int price, IntVec3 servicePoint)
    {
        var transaction = new ServiceRequest_Shop(actor, item, price, servicePoint);
        AddInt(transaction);
        this.Town.OpenTransactions.Add(actor.RefId, transaction);
        actor.Map.Events.Post(new TransactionStartedEvent(actor.Map, transaction));
        return true;
        // TODO made it a bool return in case i have some conditions that can make it fail in the future
    }

    private void AddInt(ServiceRequest_Shop transaction)
    {
        this._transactionsAll.Add(transaction);
        this._transactionsRequests.Add(transaction.Customer, transaction);
        this._transactionsByBuyer.Add(transaction.Customer, transaction);
        if (transaction.Vendor != EntityRefId.Null)
            this._transactionsBySeller.Add(transaction.Vendor, transaction);
        this._transactionsByItem.Add(transaction.Item, transaction);
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
        transaction.SetSeller(seller);
        transaction.RefreshTimer();
        this._transactionsBySeller.Add(seller.RefId, transaction);
        this._transactionsRequests.Remove(transaction.Customer);
        this.Map.Events.Post(new ShopTransactionUpdatedEvent(this.Map, transaction));
    }
    public override void Tick()
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
                this.Map.Events.Post(new TownServiceComplete(this.Map, transaction));
            }
        }
    }

    public override void Load(SaveTag tag)
    {
        tag.TryGetTagValue("ShopIDSequence", ref this.CurrentShopID);
        this.Shopss.LoadAbstract(tag, "Shops", this);
        this.Shops = this.Shopss.ToDictionary(i => i.ID, i => i);
    }

    public override void Read(IDataReader r)
    {
        this.CurrentShopID = r.ReadInt32();
        this.Shopss.ReadListAbstract(r, this);
        this.Shops = this.Shopss.ToDictionary(i => i.ID, i => i);
    }

    public void RemoveShop(int shopid)
    {
        var shop = this.Shops[shopid];
        this.Shopss.Remove(shop);
        this.Shops.Remove(shopid);
        this.Town.Net.EventOccured((int)Message.Types.ShopsUpdated, shop);
    }

    public bool ShopExists(Workplace shop)
    {
        return this.Shops.ContainsKey(shop.ID);
    }

    public override void Write(IDataWriter w)
    {
        w.Write(this.CurrentShopID);
        this.Shopss.WriteAbstract(w);
    }
    
    internal override void OnBlocksChanged(IEnumerable<IntVec3> positions)
    {
        foreach (var wp in this.Shopss)
            wp.OnBlocksChanged(positions);
    }

    internal override IEnumerable<(Func<string>, Action)> OnQuickMenuCreated()
    {
        yield return (() => "Shops", () => UIManager.ToggleSingleton<WorkplacesGui>("Shops"));
    }

    internal override void ResolveReferences()
    {
        foreach (var wp in this.Shopss)
            wp.ResolveReferences();

        foreach(var cell in this.Map.GetAllCellsWithIndex())
        {
            if (cell.cell.Block == BlockDefOf.ShopCounter.Block)
                this.ServicePoints.Add(cell.id.Local.ToGlobal(cell.chunk));
        }

        foreach (var req in this.Town.ServiceRequests.GetAllRequests<ServiceRequest_Shop>())
            this.AddInt(req);
    }

    internal void ToggleWorker(Actor a, Workplace shop)
    {
        PacketsWorkplaces.SendPlayerAssignWorkerToShop(a.Net, a.Net.GetPlayer(), shop.Map, a, shop);
    }
    protected override void AddSaveData(SaveTag tag)
    {
        tag.Add(this.CurrentShopID.Save("ShopIDSequence"));
        this.Shopss.SaveAbstract(tag, "Shops");
    }

    private ListBoxNoScroll<Workplace, Button> CreateUIShopList(Action<Workplace> selectAction, Func<Workplace, bool> filter = null)
    {
        return this.CreateUIShopList<Workplace>(selectAction, filter);
    }

    private ListBoxNoScroll<T, Button> CreateUIShopList<T>(Action<T> selectAction, Func<T, bool> filter) where T : Workplace
    {
        var shoplist = new ListBoxNoScroll<T, Button>(s => new Button(s.Name, () => selectAction?.Invoke(s)));
        shoplist.OnGameEventAction = e =>
        {
            switch ((Message.Types)e.Type)
            {
                case Message.Types.ShopsUpdated:
                    var shop = e.Parameters[0] as T;
                    if (this.Shopss.Contains(shop))
                        shoplist.AddItems(shop);
                    else
                        shoplist.RemoveItems(shop);
                    break;

                default:
                    break;
            }
        };
        shoplist.ShowAction = () =>
        {
            shoplist.Clear();
            shoplist.AddItems(this.Shops.Values.OfType<T>().Where(v => filter?.Invoke(v) ?? true).ToArray());
        };
        return shoplist;
    }

    internal void MarkPaid(Actor buyer, Entity money)
    {
        var req = this._transactionsByBuyer[buyer.RefId];
        req.Money = money.RefId;
        req.MarkPaid();
        this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));

    }
    internal void RingUp(Actor seller, Entity item)
    {
        var req = this._transactionsBySeller[seller.RefId];
        if (item.RefId != req.Item)
            throw new InvalidOperationException();
        req.RingUp();
        this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));
    }
    internal void FinishTransaction(Actor buyer)
    {
        var req = this._transactionsByBuyer[buyer.RefId];
        var shoppinglist = this._shoppingListsByActor[buyer.RefId];
        shoppinglist.MarkFulfilled();
        req.Dispose();
        this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));
    }
    internal void MarkProcessed(Actor seller)
    {
        var req = this._transactionsBySeller[seller.RefId];
        req.MarkProcessed();
        this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));
    }

    internal IEnumerable<Entity> GetItemsForSale()
        => this.Town.Map.Stockpiles.Stockpiles
        .Where(s => s.ForSale)
        .SelectMany(s => s.Items);

    internal override bool IsClaimedBySystem(Entity item)
    {
        if (item.IsForSale())
            return true;
        foreach(var t in this._transactionsAll)
        {
            if (t.Item != item.RefId)
                continue;
            if (item.Map != this.Map)
                continue;
            if (item.Cell != t.Counter.Above)
                continue;
            return true;
        }
        return false;
    }
}