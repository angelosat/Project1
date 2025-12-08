using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    class PacketWorld
    {
        static int _packetTypeId;
        static PacketWorld()
        {
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(NetEndpoint net, PlayerData player)
        {
            var server = net as Server;
            var w = server.BeginPacket(_packetTypeId);
            server.Map.World.WriteData(w);
        }
        internal static void Receive(NetEndpoint net, Packet p)
        {
            var r = p.PacketReader;
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
