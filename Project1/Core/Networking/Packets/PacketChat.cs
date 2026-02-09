using Project1.Core.Base;
using Project1.Core.Components;
using Project1.Core;
using Project1.Core.Net;

namespace Project1.Core.Net.Packets
{
    [EnsureStaticCtorCall]
    static class PacketChat
    {
        static readonly int p;
        static PacketChat()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(NetEndpoint net, int playerID, string text)
        {
            var w = net.BeginPacket(p);
            w.Write(playerID);
            w.WriteASCII(text);
        }
        internal static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var playerid = r.ReadInt32();
            var text = r.ReadASCII();
            if(net is Server)
            {
                Send(net, playerid, text);
            }
            else 
            {
                var player = net.GetPlayer(playerid);
                net.EventOccured((int)Message.Types.ChatPlayer, player, text);
            }
        }
    }
}
