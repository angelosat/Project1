using System;
using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.Core;

namespace Start_a_Town_
{
    class PacketWorld
    {
        internal static void Init()
        {
            Client.RegisterPacketHandler(PacketType.WorldData, Receive);
        }
        internal static void Send(INetwork net, PlayerData player)
        {
            var server = net as Server;
            byte[] data = Network.Serialize(server.Map.World.WriteData);

            if (player == null)
            {
                foreach (var p in server.Players.GetList())
                    server.Enqueue(p, Packet.Create(player, PacketType.WorldData, data, SendType.OrderedReliable));
            }
            else
                server.Enqueue(player, Packet.Create(player, PacketType.WorldData, data, SendType.OrderedReliable));
        }
        internal static void Receive(INetwork net, BinaryReader r)
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
