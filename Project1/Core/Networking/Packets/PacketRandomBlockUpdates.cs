using System;
using Project1.Core.Networking;
using Project1.Framework;
using Project1.Framework.Events;

namespace Project1.Core.Networking.Packets
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
