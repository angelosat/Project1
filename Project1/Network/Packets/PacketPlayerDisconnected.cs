using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketPlayerDisconnected
    {
        internal static void Send(INetPeer net, int playerID)
        {
            var server = net as Server;
            var w = server.OutgoingStream;
            w.Write(PacketType.PlayerDisconnected);
            w.Write(playerID);
        }
        internal static void Receive(INetPeer net, BinaryReader r)
        {
            var playerID = r.ReadInt32();
            (net as Client).PlayerDisconnected(playerID);
        }
        internal static void Init()
        {
            Client.RegisterPacketHandler(PacketType.PlayerDisconnected, Receive);
        }
    }
}
