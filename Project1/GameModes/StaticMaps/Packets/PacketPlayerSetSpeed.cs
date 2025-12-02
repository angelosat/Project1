using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketPlayerSetSpeed
    {
        static int p;
        internal static void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetEndpoint net, int playerID, int speed)
        {
            var w = net.GetOutgoingStreamOrderedReliable();
            w.Write(p);
            w.Write(playerID);
            w.Write(speed);
        }
        internal static void Receive(INetEndpoint net, BinaryReader r)
        {
            var playerID = r.ReadInt32();
            int speed = r.ReadInt32();
            net.SetSpeed(playerID, speed);
            if (net is Server)
                Send(net, playerID, speed);
        }
    }
}
