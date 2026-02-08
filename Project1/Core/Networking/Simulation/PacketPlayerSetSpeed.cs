using Project1.Core.Base;
using Project1.Core.Net;

namespace Project1.Core.Networking.Simulation
{
    [EnsureStaticCtorCall]
    class PacketPlayerSetSpeed
    {
        static int _pSpeedChange;
        internal static void Init()
        {
            _pSpeedChange = Registry.PacketHandlers.Register(Receive);

            Registry.PlayerInputEventHooks.Register<PlayerChangedSpeedEvent>(HandlePlayerChangedSpeed);
        }

        private static void HandlePlayerChangedSpeed(PlayerChangedSpeedEvent e)
        {
            // if server, set speed straight away
            // if client, request speed change
            Send(Client.Instance, Client.Instance.PlayerData.ID, e.Speed);
        }

        internal static void Send(NetEndpoint net, int playerID, int speed)
        {
            var w = net.BeginPacketImmediate(_pSpeedChange);
            w.Write(playerID);
            w.Write(speed);
        }
        internal static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var playerID = r.ReadInt32();
            int speed = r.ReadInt32();
            net.SetSpeed(playerID, speed);
            if (net is Server)
                Send(net, playerID, speed);
        }
    }
}
