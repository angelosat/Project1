using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketPlayerToolSwitch
    {
        static readonly int p;
        static PacketPlayerToolSwitch()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetwork net, int playerid, ControlTool tool)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(playerid);
            tool.Write(w);
        }
        internal static void Receive(INetwork net, BinaryReader r)
        {
            var plid = r.ReadInt32();
            var player = net.GetPlayer(plid);
            var tool = ControlTool.CreateOrSync(r, player);
            player.CurrentTool = tool;
            if (net is Server)
                Send(net, plid, tool);
        }
    }
}
