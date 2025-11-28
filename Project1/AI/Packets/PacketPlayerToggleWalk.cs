using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketPlayerToggleWalk
    {
        static readonly int p;
        static PacketPlayerToggleWalk()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetPeer net, bool toggle)
        {
            if (net is Server)
                throw new Exception();
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(net.GetPlayer().ID);
            w.Write(toggle);
        }
        private static void Receive(INetPeer net, BinaryReader r)
        {
            if (net is Client)
                throw new Exception();
            var pl = net.GetPlayer(r.ReadInt32());
            pl.ControllingEntity.WalkToggle(r.ReadBoolean());
        }
    }
}
