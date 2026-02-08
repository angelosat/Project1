using System.IO;
using Project1.Core.Helpers;
using Project1.Core.Net;
using Project1.Framework.IO;

namespace Project1.Core.Net.Packets
{
    class PacketPlayerDisconnected
    {
        internal static void Send(INetEndpoint net, int playerID)
        {
            var server = net as Server;
            var w = server.OutgoingStreamOrderedReliable;
            w.Write(PacketType.PlayerDisconnected);
            w.Write(playerID);
        }
        internal static void Receive(INetEndpoint net, IDataReader r)
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
