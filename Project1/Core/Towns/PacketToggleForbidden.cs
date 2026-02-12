using System.Collections.Generic;
using System.Linq;
using Project1.Framework;
using Project1.Core.Networking;
using Project1.Core.Entities;
using Project1.Framework.Events;
using Project1.Core.Networking;

namespace Project1.Core.Towns
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
            var w = net.BeginPacketNew(ReliabilityType.OrderedReliable, p);

            w.Write(instanceID);
        }
        static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var list = r.ReadListInt32();
            foreach (var id in list)
                net.World.GetEntity(id).ToggleForbidden();
            if (net is Server)
                Send(net, list);
        }
    }
}
