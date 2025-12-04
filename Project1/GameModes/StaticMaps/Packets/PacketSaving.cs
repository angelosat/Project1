using System;
using System.Collections.Generic;
using System.Text;
using Start_a_Town_.Net;


namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal class PacketSaving
    {
        static readonly int p;
        static PacketSaving()
        {
            p = NetEndpoint.RegisterPacketHandler(Receive);
        }
        public static void Send(Server server)
        {
            var w = server.BeginPacket(ReliabilityType.OrderedReliable, p);
            w.Write(server.IsSaving);
        }
        private static void Receive(NetEndpoint net, Packet packet)
        {
            ((Client)net).SetSaving(packet.Reader.ReadBoolean());
        }
    }
}
