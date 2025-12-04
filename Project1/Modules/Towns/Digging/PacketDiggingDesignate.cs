using Start_a_Town_.Net;
using System.IO;
using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketDiggingDesignate
    {
        static readonly int p;
        static PacketDiggingDesignate()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        static public void Send(INetEndpoint net, Vector3 begin, Vector3 end, bool remove)
        {
            //var stream = net.GetOutgoingStreamOrderedReliable();
            //stream.Write(p);
            var stream = net.BeginPacket(ReliabilityType.OrderedReliable, p);

            stream.Write(begin);
            stream.Write(end);
            stream.Write(remove);
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var begin = r.ReadVector3();
            var end = r.ReadVector3();
            var remove = r.ReadBoolean();
            var positions = new BoundingBox(begin, end).GetBox();
            net.EventOccured(Components.Message.Types.MiningDesignation, positions, remove);
            if (net is Server)
                Send(net, begin, end, remove);
        }
    }
}
