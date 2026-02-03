using System;
using Project1.Framework.Base;
using Start_a_Town_;

namespace Project1.Framework.Net.Packets
{
    [EnsureStaticCtorCall]
    static class PacketRandomBlockUpdates
    {
        static readonly int p;
        static PacketRandomBlockUpdates()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        static public void Send(NetEndpoint net, IntVec3[] list)
        {
            if (net is Client)
                throw new Exception();
            var strem = net.BeginPacket(p);
            strem.Write(list);
        }
        static public void Receive(NetEndpoint net, Packet packet)
        {
            var r = packet.PacketReader;
            if (net is Server)
                throw new Exception();
            var list = r.ReadListVector3();
			foreach(var vec in list)
                net.Map.RandomBlockUpdate(vec);
        }
    }
}
