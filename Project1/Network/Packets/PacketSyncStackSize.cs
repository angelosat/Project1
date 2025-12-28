using Start_a_Town_.Net;
using System;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal class PacketSyncStackSize
    {
        static readonly int _packetTypeId;
        static PacketSyncStackSize()
        {
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }
        public static void Send(GameObject entity)//, int amount)
        {
            //if (amount == 0)
            //    throw new Exception("Dispose object instead of setting stacksize to 0");
            ArgumentOutOfRangeException.ThrowIfZero(entity.StackSize);

            var server = entity.Net as Server;
            server.BeginPacket(_packetTypeId)
                .Write(entity.RefId)
                .Write(entity.StackSize);
        }
        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var entity = client.World.GetEntity(r.ReadInt32());
            //throw new NotImplementedException();
            var amount = r.ReadInt32();
            ArgumentOutOfRangeException.ThrowIfZero(amount);
            if (amount > 0) entity.Add(amount);
            else entity.Consume(amount);
        }
    }
}
