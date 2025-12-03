using System;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    class PacketMap
    {
        static readonly int p;

        static PacketMap()
        {
            p = Network.RegisterPacketHandlerWithPacket(Receive);
        }

        internal static void Send(INetEndpoint net, PlayerData player)
        {
            var server = net as Server;
            var w = server.BeginPacket(ReliabilityType.OrderedReliable, p);
            server.Map.WriteData(w);
            server.EndPacket();
        }
        private static void Receive(INetEndpoint net, Packet packet)
        {
            var r = packet.Reader;
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
