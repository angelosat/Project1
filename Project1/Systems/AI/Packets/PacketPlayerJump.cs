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
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(NetEndpoint net)
        {
            if (net is Server)
                throw new Exception();
            var pck = net.BeginPacket(_packetTypeId);
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
