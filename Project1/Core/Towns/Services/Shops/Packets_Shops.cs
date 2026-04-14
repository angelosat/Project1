using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Framework;

#nullable enable

namespace Project1.Core.Towns.Services.Shops;

[EnsureStaticCtorCall]
internal static class Packets_Shops
{
    internal readonly static PacketId 
        _pShopCreated = Registry.PacketHandlers.Register(ReceiveCreateShop), 
        _pShopDeleted, 
        _pPlayerShopCreated = Registry.PacketHandlers.Register(ReceivePlayerCreateShop),
        _pPlayerShopDeleted,
        _pPlayerToggledForSale = Registry.PacketHandlers.Register(ReceivePlayerToggledForSale),
        _pToggledForSale = Registry.PacketHandlers.Register(ReceiveToggledForSale);

    static Packets_Shops()
    {
        Registry.PlayerInputEventHooks.Register<PlayerCreateShopEvent>(HandlePlayerCreateShop);
        Registry.PlayerInputEventHooks.Register<PlayerItemToggledForSaleEvent>(HandlePlayerItemToggledForSale);

        Registry.MapEventHooksServer.Register<ItemToggledForSaleEvent>(HandleItemToggledForSale);
    }

    private static void HandleItemToggledForSale(ItemToggledForSaleEvent e)
    {
        Server.Instance.BeginPacketImmediate(_pToggledForSale)
            .Write(e.Item.RefId)
            .Write(e.ForSale);
    }
    private static void ReceiveToggledForSale(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var item = endpoint.World.Get(r.ReadEntityRefId());
        var toggle = r.ReadBoolean();
        item.Map.Town.Shops.ToggleForSale(item);

    }
    private static void HandlePlayerItemToggledForSale(PlayerItemToggledForSaleEvent e)
    {
        if(Ingame.Net.IsServer)
        {
            Ingame.Net.MainViewport.Map.Town.Shops.ToggleForSale(e.Item);
        }
        SendPlayerToggledItemForSale(e.Item);
    }

    private static void SendPlayerToggledItemForSale(Entity item)
    {
        Client.Instance.BeginPacketImmediate(_pPlayerToggledForSale)
            .Write(item.RefId);
    }
    private static void ReceivePlayerToggledForSale(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var item = endpoint.World.Get(r.ReadEntityRefId());
        item.Map.Town.Shops.ToggleForSale(item);
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
