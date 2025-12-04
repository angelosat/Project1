using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketRandomBlockUpdates
    {
        static readonly int p;
        static PacketRandomBlockUpdates()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(INetEndpoint net, IEnumerable<Vector3> list)
        {
            if (net is Client)
                throw new Exception();
            //var strem = net.GetOutgoingStreamOrderedReliable();
            //strem.Write(p);
            var strem = net.BeginPacket(ReliabilityType.OrderedReliable, p);

            strem.Write(list);
        }
        static public void Receive(INetEndpoint net, BinaryReader r)
        {
            if (net is Server)
                throw new Exception();
            var list = r.ReadListVector3();
			foreach(var vec in list)
                net.Map.RandomBlockUpdate(vec);
        }
    }
}
