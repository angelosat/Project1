using System.Collections.Generic;
using System.IO;
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
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetPeer net, IEnumerable<GameObject> enumerable)
        {
            Send(net, enumerable.Select(o => o.RefID).ToList());
        }
        internal static void Send(INetPeer net, List<int> instanceID)
        {
           
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(instanceID);
        }
        static void Receive(INetPeer net, BinaryReader r)
        {
            var list = r.ReadListInt();
            foreach (var id in list)
                net.GetNetworkEntity(id).ToggleForbidden();
            if (net is Server)
                Send(net, list);
        }
    }
}
