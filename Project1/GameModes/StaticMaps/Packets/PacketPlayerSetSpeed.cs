using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    class PacketPlayerSetSpeed
    {
        static int p;
        internal static void Init()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(NetEndpoint net, int playerID, int speed)
        {
            //var w = net.GetOutgoingStreamOrderedReliable();
            //w.Write(p);
            //var w = net.BeginPacketNew(ReliabilityType.OrderedReliable, p);

            //var w = net is Server server ? server.BeginPacketPlayerCommand(p) : net.BeginPacket(p);
            var w = net.BeginPacketImmediate(p);

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
