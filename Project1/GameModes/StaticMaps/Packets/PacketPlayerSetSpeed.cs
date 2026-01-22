using Start_a_Town_.Net;
using System;

namespace Start_a_Town_
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
            //Client.Instance.BeginPacketImmediate(_pSpeedChange)
            //    .Write(Client.Instance.PlayerData.ID)
            //    .Write(e.Speed);
        }

        internal static void Send(NetEndpoint net, int playerID, int speed)
        {
            //var w = net.GetOutgoingStreamOrderedReliable();
            //w.Write(p);
            //var w = net.BeginPacketNew(ReliabilityType.OrderedReliable, p);

            //var w = net is Server server ? server.BeginPacketPlayerCommand(p) : net.BeginPacket(p);
            var w = net.BeginPacketImmediate(_pSpeedChange);

            //$"{net.CurrentTick} : {net} sending speed: {speed}".ToConsole();
            w.Write(playerID);
            w.Write(speed);
        }
        internal static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var playerID = r.ReadInt32();
            int speed = r.ReadInt32();
            //$"{net} seting speed {speed}".ToConsole();
            net.SetSpeed(playerID, speed);
            if (net is Server)
                Send(net, playerID, speed);
        }
    }
}
