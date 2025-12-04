using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketPlayerToolSwitch
    {
        static readonly int p;
        static PacketPlayerToolSwitch()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(NetEndpoint net, int playerid, ControlTool tool)
        {
            var w = net.BeginPacketOld(p);
            w.Write(playerid);
            tool.Write(w);
        }
        internal static void Receive(NetEndpoint net, Packet packet)
        {
            var r = packet.PacketReader;
            var plid = r.ReadInt32();
            var player = net.GetPlayer(plid);
            var tool = ControlTool.CreateOrSync(r, player);
            player.CurrentTool = tool;
            if (net is Server)
                Send(net, plid, tool);
        }
    }
}
