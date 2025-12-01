using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketMap
    {
        internal static void Init()
        {
            Client.RegisterPacketHandler(PacketType.MapData, Receive);
        }
        internal static void Send(INetEndpoint net, PlayerData player)
        {
            var server = net as Server;
            byte[] data = Network.Serialize(server.Map.WriteData); // why does it let me do that?
            if (player is null)
            {
                foreach (var p in server.Players.GetList())
                    server.Enqueue(p, Packet.Create(player, PacketType.MapData, data, SendType.OrderedReliable));
            }
            else
                server.Enqueue(player, Packet.Create(player, PacketType.MapData, data, SendType.OrderedReliable));
        }
        internal static void Receive(INetEndpoint net, BinaryReader r)
        {
            var client = net as Client;
            if (client.Map is not null)
            {
                // create new empty map? or throw?
                throw new Exception("map already received");
                //"map already received, dropping packet".ToConsole();
            }
            if (client.World is null)
                throw new Exception("map received before world");

            StaticMap map = StaticMap.ReadData(client, r);
            map.World = client.World as StaticWorld;
            map.World.GetMaps().Add(map.Coordinates, map);
            client.Map = map;
            GameMode.Current.MapReceived(map);
        }
    }
}
