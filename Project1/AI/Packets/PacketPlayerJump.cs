using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketPlayerJump
    {
        static readonly int _packetTypeId;
        static PacketPlayerJump()
        {
            _packetTypeId = NetEndpoint.RegisterPacketHandler(Receive);
        }
        internal static void Send(NetEndpoint net)
        {
            if (net is Server)
                throw new Exception();
            //var w = net.GetOutgoingStreamOrderedReliable();
            //w.Write(p);

            //var w = net.BeginPacket(ReliabilityType.OrderedReliable, p);
            //w.Write(net.GetPlayer().ID);
            //net.EndPacket();

            var pck = net.BeginPacketNew(ReliabilityType.OrderedReliable, _packetTypeId);
            pck.Write(net.GetPlayer().ID);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            if (net is Client)
                throw new Exception();
            var r = pck.PacketReader;
            var pl = net.GetPlayer(r.ReadInt32());
            pl.ControllingEntity.Jump();
        }
    }
}
