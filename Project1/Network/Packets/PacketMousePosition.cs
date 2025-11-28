using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketMousePosition
    {
        static readonly int p;
        static PacketMousePosition()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static internal void Send(INetPeer net, int playerid, TargetArgs target)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(playerid);
            target.Write(w);
        }
        static internal void Receive(INetPeer net, BinaryReader r)
        {
            var playerid = r.ReadInt32();
            var target = TargetArgs.Read(net.Map, r);
            net.GetPlayer(playerid)?.UpdateTarget(target);
            if (net is Server)
                Send(net, playerid, target);
        }
    }
}
