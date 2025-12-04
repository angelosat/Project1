using System;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntitySync
    {
        static readonly int _packetTypeId;
        static PacketEntitySync()
        {
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }
        static public void Send(NetEndpoint net, GameObject entity)
        {
            if (net is Client)
                throw new Exception();
            var w = net.BeginPacketOld(_packetTypeId);

            w.Write(entity.RefId);
            entity.SyncWrite(w);
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            if (net is Server)
                throw new Exception();
            var entity = net.World.GetEntity(r.ReadInt32());
            entity.SyncRead(r);
        }
    }
}
