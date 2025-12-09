using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal class PacketSetStackSize
    {
        static readonly int _packetTypeId;
        static PacketSetStackSize()
        {
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }
        public static void Send(GameObject entity, int amount)
        {
            if (amount == 0)
                throw new Exception("Dispose object instead of setting stacksize to 0");
            var server = entity.Net as Server;
            server.BeginPacket(_packetTypeId)
                .Write(entity.RefId)
                .Write(amount);
        }
        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var entity = client.World.GetEntity(r.ReadInt32());
            entity.StackSize = r.ReadInt32();
        }
    }
}
