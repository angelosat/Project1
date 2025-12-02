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
            p = Network.RegisterPacketHandlerWithPacket(Receive);
        }
        public static void Send(Server server)
        {
            var w = server[ReliabilityType.OrderedReliable];
            w.Write(p);
            w.Write(server.IsSaving);
        }
        private static void Receive(INetEndpoint net, Packet packet)
        {
            ((Client)net).SetSaving(packet.Reader.ReadBoolean());
        }
    }
}
