using Project1.Core.Simulation;
using Project1.Framework;
using System;
using System.Linq;

namespace Project1.Core.Networking.Simulation;

[EnsureStaticCtorCall]
class PacketMap
{
    static readonly int _packetTypeId;

    static PacketMap()
    {
        _packetTypeId = Registry.PacketHandlers.Register(Receive);
    }

    internal static void Send(NetEndpoint net, PlayerData player)
    {
        var server = net as Server;
        var w = player.BeginReliable(_packetTypeId);
        server.World.MainMap.WriteData(w);
    }
    private static void Receive(NetEndpoint net, Packet packet)
    {
        var r = packet.PacketReader;
        var client = net as Client;
        //if (client.Map is not null)
        //    throw new Exception("map already received");
        if (client.World is null)
            throw new Exception("map received before world");

        StaticMap map = StaticMap.ReadData(client, r);
        if(client.World.Maps.Any(m=>m.ID == map.ID))
            throw new Exception("map already received");
        map.World = client.World as StaticWorld;
        client.PendingMaps.Add(map.ID, map);
        GameMode.Current.MapReceived(map);
    }
}
