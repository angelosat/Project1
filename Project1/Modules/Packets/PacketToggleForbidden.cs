using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketToggleForbidden
    {
        static readonly int p;
        static PacketToggleForbidden()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(NetEndpoint net, IEnumerable<GameObject> enumerable)
        {
            Send(net, enumerable.Select(o => o.RefId).ToList());
        }
        internal static void Send(NetEndpoint net, List<int> instanceID)
        {
           
            //var w = net.GetOutgoingStreamOrderedReliable();
            //w.Write(p);
            var w = net.BeginPacketNew(ReliabilityType.OrderedReliable, p);

            w.Write(instanceID);
        }
        static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var list = r.ReadListInt();
            foreach (var id in list)
                net.World.GetEntity(id).ToggleForbidden();
            if (net is Server)
                Send(net, list);
        }
    }
}
