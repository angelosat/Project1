using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketChat
    {
        static readonly int p;
        static PacketChat()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetEndpoint net, int playerID, string text)
        {
            //var w = net.GetOutgoingStreamOrderedReliable();
            //w.Write(p);
            var w = net.BeginPacket(ReliabilityType.OrderedReliable, p);

            w.Write(playerID);
            w.WriteASCII(text);
        }
        internal static void Receive(INetEndpoint net, BinaryReader r)
        {
            var playerid = r.ReadInt32();
            var text = r.ReadASCII();
            if(net is Server)
            {
                Send(net, playerid, text);
            }
            else 
            {
                var player = net.GetPlayer(playerid);
                net.EventOccured(Components.Message.Types.ChatPlayer, player, text);
            }
        }
    }
}
