using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.Towns.Stockpiles;
using Project1.Framework;
using System;

namespace Project1.Core.Towns.Shops;

[EnsureStaticCtorCall]
public class PacketsWorkplaces
{
    static readonly int PacketPlayerCreateShop, PacketPlayerDeleteShop, PacketPlayerAddStockpileToShop, PacketPlayerAddShoppingArea, PacketPlayerAssignWorkerToShop, PacketPlayerShopAssignCounter;//, PacketPlayerRenameShop;
    static PacketsWorkplaces()
    {
        PacketPlayerCreateShop = Registry.PacketHandlers.Register(ReceivePlayerCreateShop);
        PacketPlayerDeleteShop = Registry.PacketHandlers.Register(ReceivePlayerDeleteShop);
        PacketPlayerAddStockpileToShop = Registry.PacketHandlers.Register(ReceivePlayerAddStockpileToShop);
        PacketPlayerAddShoppingArea = Registry.PacketHandlers.Register(ReceivePlayerAddShoppingArea);
        PacketPlayerAssignWorkerToShop = Registry.PacketHandlers.Register(HandlePlayerAssignWorkerToShop);
        PacketPlayerShopAssignCounter = Registry.PacketHandlers.Register(ReceivePlayerShopAssignCounter);
    }

    public static void SendPlayerDeleteShop(NetEndpoint net, PlayerData player, MapId mapid, int shopid)
    {
        if (net is Server)
        {
            net.World.Get(mapid).Town.ShopManager.RemoveShop(shopid);
        }
        net.BeginPacket(PacketPlayerDeleteShop)
            .Write(player.ID)
            .Write(mapid)
            .Write(shopid);
    }
    private static void ReceivePlayerDeleteShop(NetEndpoint net, Packet pck)
    {
        var r = pck.PacketReader;
        var pl = net.GetPlayer(r.ReadInt32());
        var mapid = r.ReadMapId();
        var map = net.World.Get(mapid);
        var shopid = r.ReadInt32();
        if (net is Client)
            map.Town.ShopManager.RemoveShop(shopid);
        else
            SendPlayerDeleteShop(net, pl, mapid, shopid);
    }

    static public void SendPlayerShopAssignCounter(NetEndpoint net, PlayerData player, MapBase map, Workplace shop, IntVec3 global)
    {
        var w = net.BeginPacket(PacketPlayerShopAssignCounter);
        w.Write(player.ID);
        w.Write(map.ID);
        w.Write(shop?.ID ?? -1);
        w.Write(global);
    }
    static void ReceivePlayerShopAssignCounter(NetEndpoint net, Packet pck)
    {
        var r = pck.PacketReader;
        var player = net.GetPlayer(r.ReadInt32());
        var map = net.World.Get(r.ReadMapId());
        var manager = map.Town.ShopManager;
        var shop = manager.GetShop(r.ReadInt32());
        var global = r.ReadIntVec3();
        if (shop != null)
        {
            if (global.Z < 0)
                throw new NotImplementedException();
            shop.AddFacility(global);
        }
        else
        {
            throw new NotImplementedException();
        }
        if (net is Server)
            SendPlayerShopAssignCounter(net, player, map, shop, global);
    }

    static public void SendPlayerAssignWorkerToShop(NetEndpoint net, PlayerData player, MapBase map, Actor actor, Workplace shop)
    {
        var w = net.BeginPacket(PacketPlayerAssignWorkerToShop);
        w.Write(player.ID);
        w.Write(map.ID);
        w.Write(actor.RefId);
        w.Write(shop.ID);
    }
    private static void HandlePlayerAssignWorkerToShop(NetEndpoint net, Packet pck)
    {
        var r = pck.PacketReader;
        var playerID = r.ReadInt32();
        var map = net.World.Get(r.ReadMapId());
        var actorID = r.ReadInt32();
        var shopID = r.ReadInt32();
        var manager = map.Town.ShopManager;
        var actor = net.World.GetEntity(actorID) as Actor;
        var shop = manager.GetShop(shopID);
        shop.AddWorker(actor);
        if (net is Server)
            SendPlayerAssignWorkerToShop(net, net.GetPlayer(playerID), map, actor, shop);
    }

    static public void SendPlayerAddStockpileToShop(NetEndpoint net, int playerID, MapBase map, int shopID, int stockpileID)
    {
        if (shopID < 0)
            return;
        var w = net.BeginPacket(PacketPlayerAddStockpileToShop);
        w.Write(playerID);
        w.Write(map.ID);
        w.Write(shopID);
        w.Write(stockpileID);
    }
    private static void ReceivePlayerAddStockpileToShop(NetEndpoint net, Packet pck)
    {
        var r = pck.PacketReader;
        var playerID = r.ReadInt32();
        var map = net.World.Get(r.ReadMapId());
        var shopid = r.ReadInt32();
        var stockpileid = r.ReadInt32();
        var shopmanager = map.Town.ShopManager;
        var stockpile = map.Town.ZoneManager.GetZone<Stockpile>(stockpileid);
        var shop = shopmanager.GetShop(shopid) as Shop;
        shop.AddStockpile(stockpile);

        if (net is Server)
            SendPlayerAddStockpileToShop(net, playerID, map, shopid, stockpileid);
    }

    static public void SendPlayerAddShoppingArea(NetEndpoint net, int playerID, MapId mapid, int shopID, int stockpileID)
    {
        if (shopID < 0)
            return;
        var w = net.BeginPacket(PacketPlayerAddShoppingArea);

        w.Write(playerID);
        w.Write(mapid);
        w.Write(shopID);
        w.Write(stockpileID);
    }
    private static void ReceivePlayerAddShoppingArea(NetEndpoint net, Packet pck)
    {
        var r = pck.PacketReader;
        var playerID = r.ReadInt32();
        var mapid = r.ReadMapId();
        var map = net.World.Get(mapid);
        var shopid = r.ReadInt32();
        var stockpileid = r.ReadInt32();
        var shopmanager = map.Town.ShopManager;
        var stockpile = map.Town.ZoneManager.GetZone<Stockpile>(stockpileid);
        var shop = shopmanager.GetShop(shopid) as Shop;
        shop.ToggleShoppingArea(stockpile);

        if (net is Server)
            SendPlayerAddShoppingArea(net, playerID, mapid, shopid, stockpileid);
    }


    static public void SendPlayerCreateShop(NetEndpoint net, int playerID, MapBase map, Type shopType, int shopID = 0)
    {
        var w = net.BeginPacket(PacketPlayerCreateShop);
        w.Write(playerID);
        w.Write(map.ID);
        w.Write(shopType.FullName);
        w.Write(shopID);
    }
    private static void ReceivePlayerCreateShop(NetEndpoint net, Packet pck)
    {
        var r = pck.PacketReader;
        var playerID = r.ReadInt32();
        var map = net.World.Get(r.ReadMapId());
        var shoptypename = r.ReadString();

        var shopid = r.ReadInt32();
        var manager = map.Town.ShopManager;

        if (net is Client)
            manager.CurrentShopID = shopid;
        var shoptype = Type.GetType(shoptypename);
        var id = manager.GetNextShopID();
        var workplace = Activator.CreateInstance(shoptype, manager, id) as Workplace;
        manager.AddShop(workplace);
        if (net is Server)
            SendPlayerCreateShop(net, playerID, map, shoptype, workplace.ID);
    }
}
