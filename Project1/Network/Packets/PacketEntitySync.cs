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
            _packetTypeId = PacketRegistry.Register(Receive);
        }
        static public void Send(INetEndpoint net, GameObject entity)
        {
            if (net is Client)
                throw new Exception();
            //var w = net.GetOutgoingStreamOrderedReliable();
            //w.Write((int)PckType);
            var w = net.BeginPacket(ReliabilityType.OrderedReliable, _packetTypeId);

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
