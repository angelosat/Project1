using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal class PacketRegisterEntity
    {
        static readonly int _packetTypeId;
        static PacketRegisterEntity()
        {
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }

        public static void Send(GameObject obj)
        {
            var server = obj.Net as Server;
            obj.Write(server.BeginPacket(_packetTypeId));
        }
        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var obj = GameObject.Create(r);
            client.World.Register(obj);
        }
    }
}
