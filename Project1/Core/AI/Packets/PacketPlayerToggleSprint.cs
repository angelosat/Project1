using Project1.Core.Networking;
using Project1.Framework;
using System;

namespace Project1.Core.AI.Packets
{
    [EnsureStaticCtorCall]
    static class PacketPlayerToggleSprint
    {
        static readonly int p;
        static PacketPlayerToggleSprint()
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
            if (net.IsClient)
                throw new Exception();
            var pl = net.GetPlayer(r.ReadInt32());
            pl.ControllingEntity.SprintToggle(r.ReadBoolean());
        }
    }
}
