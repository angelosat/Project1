using System;
using Project1.Core.Base;
using Project1.Core.Net;
using Project1.Core.Net;
using Project1.Core.Simulation;

namespace Project1.Core.Net.Simulation
{
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
            server.Map.WriteData(w);
        }
        private static void Receive(NetEndpoint net, Packet packet)
        {
            var r = packet.PacketReader;
            var client = net as Client;
            if (client.Map is not null)
            {
                throw new Exception("map already received");
            }
            if (client.World is null)
                throw new Exception("map received before world");

            StaticMap map = StaticMap.ReadData(client, r);
            map.World = client.World as StaticWorld;
            map.World.GetMaps().Add(map.Coordinates, map);
            client.SetMap(map);
            GameMode.Current.MapReceived(map);
        }
    }
}
