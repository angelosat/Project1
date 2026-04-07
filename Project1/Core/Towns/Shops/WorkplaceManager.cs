using Project1.Core.Blocks;
using Project1.Core.Components;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Resources;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Towns.Services;
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

namespace Project1.Core.Towns.Shops;

internal record struct TransactionStartedEvent(MapBase Map, ShopTransaction Transaction) : IEventPayload { }
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
            Ingame.MainViewportMap.Town.ShopManager.CreateShop();
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
        var shop = map.Town.ShopManager.CreateShop();
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
        map.Town.ShopManager.CreateShop(shopid);
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
public partial class TownServicesComp : TownComponent
{
    public ChangeNotifier Notifications = new();
    const int UIListWidth = 250;

    readonly ObservableHashSet<Workplace> Shopss = new();

    readonly Dictionary<TownServiceDef, TownServiceRuntime> _servicesByDef = [];
    readonly Dictionary<Type, TownServiceRuntime> _servicesByType = [];
    readonly HashSet<IntVec3> ServicePoints = [];
    public IReadOnlySet<IntVec3> GetServicePoints()
        => this.ServicePoints;
    public bool HasServicePoints => this.ServicePoints.Count > 0;
    internal int CurrentShopID = 1;

    Dictionary<int, Workplace> Shops = [];

    static TownServicesComp()
    {
        Tavern.Init();
    }

    public TownServicesComp(Town town) : base(town)
    {
        foreach (var def in Def.Get<TownServiceDef>())
        {
            var serviceType = def.RuntimeType;
            var runtime = def.CreateRuntime();
            this._servicesByDef.Add(def, runtime);
            this._servicesByType.Add(serviceType, runtime);
        }

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

    public T GetService<T>() where T : TownServiceRuntime => (T)this._servicesByType[typeof(T)];
    public override string Name => "Shops";
    public Shop CreateShop()
    {
        var newshop = new Shop(this, this.GetNextShopID());
        //this.Shops.Add(newshop.ID, newshop);
        this.AddShop(newshop);
        return newshop;
    }
    public Shop CreateShop(int id)
    {
        var newshop = new Shop(this, id);
        //this.Shops.Add(newshop.ID, newshop);
        this.AddShop(newshop);
        return newshop;
    }
    public void AddShop(Workplace shop)
    {
        this.Shopss.Add(shop);
        this.Shops.Add(shop.ID, shop);
        this.Notifications.Notify();
        //this.Town.Net.EventOccured((int)Message.Types.ShopsUpdated, shop);
    }
    
    public Workplace FindShop(Stockpile stockpile)
    {
        return this.Shopss.FirstOrDefault(sh => sh.HasStockpile(stockpile.ID));
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

    public Workplace GetShop(IntVec3 facility)
    {
        return this.Shopss.FirstOrDefault(s => s.GetFacilities().Any(f => f == facility));
    }

    public Workplace GetShop(Actor worker)
    {
        return this.Shopss.FirstOrDefault(s => s.HasWorker(worker));
    }

    public T GetShop<T>(Actor worker) where T : Workplace
    {
        return this.Shopss.FirstOrDefault(s => s.HasWorker(worker)) as T;
    }

    public T GetShop<T>(int shopid) where T : Workplace
    {
        return this.Shops[shopid] as T;
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

    readonly Dictionary<EntityRefId, ShopTransaction> _transactionsRequests = [];
    readonly Dictionary<EntityRefId, ShopTransaction> _transactionsBySeller = [];
    readonly Dictionary<EntityRefId, ShopTransaction> _transactionsByBuyer = [];
    readonly Dictionary<EntityRefId, ShopTransaction> _transactionsActive = [];
    readonly Dictionary<EntityRefId, ShopTransaction> _transactionsByItem = [];
    readonly List<ShopTransaction> _transactionsAll = [];
    internal bool TryBeginTransaction(Actor actor, Entity item, int price, IntVec3 servicePoint)
    {
        var transaction = new ShopTransaction(this.Map.World.CurrentTick, (int)actor.Resources.GetValue(ResourceDefOf.Patience), actor, item, price, servicePoint);
        this._transactionsAll.Add(transaction);
        this._transactionsRequests.Add(actor.RefId, transaction);
        this._transactionsByBuyer.Add(actor.RefId, transaction);
        this._transactionsByItem.Add(item.RefId, transaction);
        this.Town.OpenTransactions.Add(actor.RefId, transaction);
        actor.Map.Events.Post(new TransactionStartedEvent(actor.Map, transaction));
        return true;
        // TODO made it a bool return in case i have some conditions that can make it fail in the future
    }
    internal bool TryGetTransaction(EntityRefId buyer, out ShopTransaction transaction)
      => this._transactionsByBuyer.TryGetValue(buyer, out transaction);
    internal bool TryGetTransaction(Actor buyer, out ShopTransaction transaction)
        => this._transactionsByBuyer.TryGetValue(buyer.RefId, out transaction);
    internal ShopTransaction GetTransaction(Actor buyer)
        => this._transactionsByBuyer.TryGetValue(buyer.RefId, out var t) ? t : null;
    
    internal bool TryGetTransactionBySeller(Actor seller, out ShopTransaction transaction)
        => this._transactionsBySeller.TryGetValue(seller.RefId, out transaction);
    internal bool TryGetTransactionByItem(Entity item, out ShopTransaction transaction)
        => this._transactionsByItem.TryGetValue(item.RefId, out transaction);
    internal ShopTransaction GetTransactionBySeller(Actor seller)
           => this._transactionsBySeller.TryGetValue(seller.RefId, out var t) ? t : null;
    internal IEnumerable<ShopTransaction> PendingTransactions => this._transactionsRequests.Values;
    internal void AssignSeller(ShopTransaction transaction, Actor seller)
    {
        if (transaction.Seller != EntityRefId.Null)
            throw new Exception();
        transaction.Seller = seller.RefId;
        transaction.RefreshTimer();
        this._transactionsBySeller.Add(seller.RefId, transaction);
        this._transactionsRequests.Remove(transaction.Buyer);
        this.Map.Events.Post(new ShopTransactionUpdatedEvent(this.Map, transaction));
    }
    public override void Tick()
    {
        foreach(var transaction in this._transactionsAll.ToArray())
        {
            if (transaction.IsFailed || transaction.IsSucceeded)
            {
                this._transactionsAll.Remove(transaction);
                this._transactionsActive.Remove(transaction.Buyer);
                this._transactionsRequests.Remove(transaction.Buyer);
                this._transactionsByBuyer.Remove(transaction.Buyer);
                this._transactionsByItem.Remove(transaction.Item);
                this.Town.OpenTransactions.Remove(transaction.Buyer);
                if (transaction.Seller != EntityRefId.Null)
                    this._transactionsBySeller.Remove(transaction.Seller);
                this.Map.Events.Post(new ShopTransactionFinishedEvent(this.Map, transaction));
            }
        }
    }
    public Control GetUIShopListWithNoneOption<T>(Action<Workplace> selectAction, Func<T, bool> filter) where T : Workplace // TODO make this a singleton
    {
        var box = new GroupBox();
        void action(Workplace wp)
        {
            selectAction(wp);
            box.TopLevelControl.Hide();
        };
        return box.AddControlsVertically(
                new Button("None", () => action(null), UIListWidth),
                this.CreateUIShopList(action, filter))
            .ToPanelLabeled("Select shop").HideOnAnyClick();
    }

    public override void Load(SaveTag tag)
    {
        tag.TryGetTagValue("ShopIDSequence", ref this.CurrentShopID);
        this.Shopss.LoadAbstract(tag, "Shops", this);
        this.Shops = this.Shopss.ToDictionary(i => i.ID, i => i);
    }

    public void PlayerAssignCounter(Workplace shop, IntVec3 global)
    {
        var net = this.Town.Net;
        PacketsWorkplaces.SendPlayerShopAssignCounter(net, net.GetPlayer(), shop.Map, shop, global);
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

    internal IEnumerable<T> GetShops<T>() where T : Workplace
    {
        return this.Shopss.OfType<T>();
    }
    internal override void OnBlocksChanged(IEnumerable<IntVec3> positions)
    {
        foreach (var wp in this.Shopss)
            wp.OnBlocksChanged(positions);
    }

    internal override IEnumerable<(Func<string>, Action)> OnQuickMenuCreated()
    {
        //var win = new Lazy<Window>(() => this.GetUIManager().ToWindow("Shops"));
        //yield return new Tuple<Func<string>, Action>(() => "Businesses", () => win.Value.Toggle());
        yield return (() => "Shops", () => UIManager.ToggleSingleton<WorkplacesGui>("Shops"));
    }
    internal /*override*/ void OnTargetSelected(IUISelection info, ISelectable selected)
    {
        if (selected is Stockpile stockpile)
        {
            var net = stockpile.Town.Net;

            var control = new Lazy<Control>(
                () =>
                new GroupBox().AddControlsVertically(
                    new Button("None", () => PacketsWorkplaces.SendPlayerAddStockpileToShop(net, net.GetPlayer().ID, stockpile.Map, stockpile.Town.ShopManager.FindShop(stockpile)?.ID ?? -1, stockpile.ID), UIListWidth),
                    this.CreateUIShopList(sh => PacketsWorkplaces.SendPlayerAddStockpileToShop(net, net.GetPlayer().ID, sh.Map, sh.ID, stockpile.ID)))
                .ToPanelLabeled("Select shop").HideOnAnyClick());

            info.AddTabAction("Shop", () => control.Value.SetLocation(UIManager.Mouse).Toggle());

            //info.AddInfo(new Label(() => string.Format("Shop: {0}", this.Shopss.FirstOrDefault(sh => sh.HasStockpile(stockpile.ID))?.Name ?? "")));
            info.AddInfo(new Label(() => $"Shop: {this.Shopss.FirstOrDefault(sh => sh.HasStockpile(stockpile.ID))?.Name ?? ""}"));
        }
        else if (selected is InteractionTarget target)
        {
            if (target.Type == TargetType.Cell)
            {
                var block = target.Block;
                if (this.Shopss.Any(s => s.IsAllowed(block)))
                {
                    info.AddTabAction("Shopp", () =>
                    {
                        //var foundShop = this.Shopss.FirstOrDefault(w => w.Facilities.ContainsKey(target.Global));
                        var foundShop = this.Shopss.FirstOrDefault(w => w.Facilities.Contains(target.Global));

                        if (foundShop == null)
                            this.GetUIShopListWithNoneOption<Workplace>(s => this.PlayerAssignCounter(s, target.Global), w => w.IsAllowed(block)).SetLocation(UIManager.Mouse).Toggle();
                        else
                            foundShop.OpenGui();
                    });
                }
            }
        }
        
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

    GroupBox CreateShopListGui()
    {
        var box = new GroupBox();
        var table = new TableCompact<Workplace>().AddColumn(null, "name", 200, w => new Label(() => w.Name)).Bind(this.Shopss);
        box.AddControls(table);
        return box;
    }
    private ListBoxNoScroll<Workplace, Button> CreateUIShopList(Action<Workplace> selectAction, Func<Workplace, bool> filter = null)
    {
        return this.CreateUIShopList<Workplace>(selectAction, filter);
    }

    private ListBoxNoScroll<T, Button> CreateUIShopList<T>(Action<T> selectAction, Func<T, bool> filter) where T : Workplace
    {
        //var shoplist = new ListBoxNew<T, Button>(UIListWidth, Button.DefaultHeight * 8, s => new Button(s.Name, () => selectAction?.Invoke(s)));
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

    Control GetUIManager()
    {
        var box = new GroupBox();
        var boxList = new GroupBox();

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
                        () => PacketsWorkplaces.SendPlayerDeleteShop(this.Town.Net, this.Town.Net.GetPlayer(), this.Map.ID, w.ID))));

        var shoplistcontainer = shoplist.MakeScrollable(-1, 200);

        shoplist.OnGameEventAction = e =>
        {
            switch ((Message.Types)e.Type)
            {
                case Message.Types.ShopsUpdated:
                    var shop = e.Parameters[0] as Workplace;
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
            shoplist.ClearItems();
            shoplist.AddItems(this.Shops.Values.ToArray());
        };
        shoplist.AddItems(this.Shops.Values.ToArray());
        var net = this.Town.Net;
        var selectTypeMenu = selectShopType(t => PacketsWorkplaces.SendPlayerCreateShop(net, net.GetPlayer().ID, this.Map, t));
        var btnNew = new Button("New", () => selectTypeMenu.Toggle(UIManager.MouseScaled));
        boxList.AddControlsVertically(shoplistcontainer, btnNew);
        box.AddControlsHorizontally(boxList);
        return box;

        Control selectShopType(Action<Type> callback)
        {
            var list = new ListBoxNoScroll<Type, Button>(t => new Button(t.Name, () => callback(t)));
            list.AddItems(typeof(Shop), typeof(Tavern));
            return list.ToContextMenu("Select shop type");
        }
    }

    internal void MarkPaid(Actor buyer, Entity money)
    {
        var t = this._transactionsByBuyer[buyer.RefId];
        t.Money = money.RefId;
        t.MarkPaid();
        this.Map.Events.Post(new ShopTransactionUpdatedEvent(this.Map, t));
    }
    internal void RingUp(Actor seller, Entity item)
    {
        var t = this._transactionsBySeller[seller.RefId];
        if (item.RefId != t.Item)
            throw new InvalidOperationException();
        t.RingUp();
        this.Map.Events.Post(new ShopTransactionUpdatedEvent(this.Map, t));
    }
    internal void FinishTransaction(Actor buyer)
    {
        var t = this._transactionsByBuyer[buyer.RefId];
        var shoppinglist = this._shoppingListsByActor[buyer.RefId];
        //t.MarkComplete();
        shoppinglist.MarkFulfilled();
        t.Dispose();
        this.Map.Events.Post(new ShopTransactionUpdatedEvent(this.Map, t));
        //this.Map.Events.Post(new ShopTransactionFinishedEvent(this.Map, t));
    }
    internal void MarkProcessed(Actor seller)
    {
        var t = this._transactionsBySeller[seller.RefId];
        t.MarkProcessed();
        this.Map.Events.Post(new ShopTransactionUpdatedEvent(this.Map, t));
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