using Microsoft.Xna.Framework;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Core.Networking;
using Project1.Core.Components;
using Project1.Core.Helpers;

namespace Project1.Core.Towns.Digging
{
    [EnsureStaticCtorCall]
    static class PacketDiggingDesignate
    {
        static readonly int p;
        static PacketDiggingDesignate()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        static public void Send(NetEndpoint net, Vector3 begin, Vector3 end, bool remove)
        {
            var stream = net.BeginPacketNew(ReliabilityType.OrderedReliable, p);

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
            net.EventOccured((int)Message.Types.MiningDesignation, positions, remove);
            if (net.IsServer)
                Send(net, begin, end, remove);
        }
    }
}
