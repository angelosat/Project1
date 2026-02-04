using Project1.Framework.Base;
using Start_a_Town_;

namespace Project1.Framework.Net.Packets
{
    [EnsureStaticCtorCall]
    static class PacketPlayerConnecting
    {
        static readonly PacketId _pType;
        static PacketPlayerConnecting()
        {
            _pType = Registry.PacketHandlers.Register(OnPlayerConnecting);
        }
        internal static void Send(NetEndpoint net, PlayerData player)
        {
            var w = net.BeginPacketImmediate(_pType);
            player.Write(w);
        }
        internal static void Receive(INetEndpoint net, IDataReader r)
        {
            PlayerData player = PlayerData.Read(r);
            var client = net as Client;
            client.AddPlayer(player);
        }
        static void OnPlayerConnecting(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            PlayerData player = PlayerData.Read(r);
            var client = endpoint as Client;
            client.AddPlayer(player);
        }
    }
}
