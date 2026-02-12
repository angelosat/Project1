using Project1.Framework;
using Project1.Core.Input;
using Project1.Framework.Events;
using Project1.Core.Networking;

namespace Project1.Core.Networking.Packets
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
