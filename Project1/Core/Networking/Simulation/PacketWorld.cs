using System;
using Project1.Framework;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Framework.Events;

namespace Project1.Core.Networking.Simulation
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
            var w = player.BeginReliable(_packetTypeId);
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
            client.SetWorld(world);
        }
    }
}
