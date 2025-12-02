using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    class PacketWorld
    {
        static int p;
        static PacketWorld()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetEndpoint net, PlayerData player)
        {
            var server = net as Server;
            var w = server[ReliabilityType.OrderedReliable];
            w.Write(p);
            server.Map.World.WriteData(w);
        }
        internal static void Receive(INetEndpoint net, BinaryReader r)
        {
            var client = net as Client;
            if (client.World != null)
            {
                throw new Exception("world already received");
                //"world already received, dropping packet".ToConsole();
            }
            var world = new StaticWorld(r);
            client.World = world;
            world.Net = client;
        }
    }
}
