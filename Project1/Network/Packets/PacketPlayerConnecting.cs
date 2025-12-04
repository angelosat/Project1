using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    static class PacketPlayerConnecting
    {
        static PacketPlayerConnecting()
        {

        }
        internal static void Init()
        {
            Client.RegisterPacketHandler(PacketType.PlayerConnecting, Receive);
        }
        internal static void Send(INetEndpoint net, PlayerData player)
        {
            var w = (net as Server).OutgoingStreamOrderedReliable;
            w.Write(PacketType.PlayerConnecting);
            player.Write(w);
        }
        internal static void Receive(INetEndpoint net, IDataReader r)
        {
            PlayerData player = PlayerData.Read(r);
            var client = net as Client;
            client.AddPlayer(player);
        }
    }
}
