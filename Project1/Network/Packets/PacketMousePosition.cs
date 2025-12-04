using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketMousePosition
    {
        static readonly int _packetTypeId;
        static PacketMousePosition()
        {
            _packetTypeId = PacketRegistry.Register(Receive);
        }
        static internal void Send(INetEndpoint net, int playerid, TargetArgs target)
        {
            //var w = net.GetOutgoingStreamOrderedReliable();
            //w.Write(p);
            var w = net.BeginPacket(ReliabilityType.OrderedReliable, _packetTypeId);

            w.Write(playerid);
            target.Write(w);
        }
        static internal void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var playerid = r.ReadInt32();
            var target = TargetArgs.Read(net, r);
            net.GetPlayer(playerid)?.UpdateTarget(target);
            if (net is Server)
                Send(net, playerid, target);
        }
    }
}
