using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketMousePosition
    {
        static readonly int _packetTypeId;
        static PacketMousePosition()
        {
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }
        static internal void Send(NetEndpoint net, int playerid, TargetArgs target)
        {
            var w = net.BeginPacket(_packetTypeId);
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
