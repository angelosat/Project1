using System;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketPlayerToggleMove
    {
        static readonly int p;
        static PacketPlayerToggleMove()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(NetEndpoint net, bool toggle)
        {
            if (net is Server)
                throw new Exception();
            var w = net.BeginPacket(p);
            w.Write(net.GetPlayer().ID);
            w.Write(toggle);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            if (net is Client)
                throw new Exception();
            var pl = net.GetPlayer(r.ReadInt32());
            pl.ControllingEntity.MoveToggle(r.ReadBoolean());
        }
    }
}
