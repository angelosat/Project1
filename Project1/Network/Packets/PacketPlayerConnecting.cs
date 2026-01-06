using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketPlayerConnecting
    {
        static readonly PacketId _pType;
        static PacketPlayerConnecting()
        {
            _pType = Registry.PacketHandlers.Register(OnPlayerConnecting);
        }
        //internal static void Init()
        //{
            //Client.RegisterPacketHandler(PacketType.PlayerConnecting, Receive);
        //}
        internal static void Send(NetEndpoint net, PlayerData player)
        {
            //var w = (net as Server).OutgoingStreamOrderedReliable;
            //w.Write(PacketType.PlayerConnecting);
            //player.Write(w);
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
