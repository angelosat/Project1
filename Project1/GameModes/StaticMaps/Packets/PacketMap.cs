using System;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    class PacketMap
    {
        static readonly int _packetTypeId;

        static PacketMap()
        {
            //_packetTypeId = NetEndpoint.RegisterPacketHandler(Receive);
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }

        internal static void Send(NetEndpoint net, PlayerData player)
        {
            var server = net as Server;
            var w = server.BeginPacketOld(_packetTypeId);
            server.Map.WriteData(w);
        }
        private static void Receive(NetEndpoint net, Packet packet)
        {
            var r = packet.PacketReader;
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
