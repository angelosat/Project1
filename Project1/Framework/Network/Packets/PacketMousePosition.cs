using Project1.Framework.Net;
using Start_a_Town_;

namespace Project1.Framework.Net.Packets
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
            var w = net.BeginPacketImmediate(_packetTypeId);
            w.Write(playerid);
            target.Write(w);
        }
        static internal void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var playerid = r.ReadInt32();
            var target = TargetArgs.Read(net, r);
            net.GetPlayer(playerid)?.UpdateTarget(target);
            if (net is Server server)
                Send(server, playerid, target);
        }
    }
}
