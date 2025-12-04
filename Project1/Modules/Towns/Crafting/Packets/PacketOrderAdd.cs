using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketOrderAdd
    {
        static int p;
        static internal void Init()
        {
            // TODO
            p = Network.RegisterPacketHandler(Receive);
        }

        internal static void Send(INetEndpoint net, Vector3 global, Reaction reaction)
        {
            //var w = net.GetOutgoingStreamOrderedReliable();
            //w.Write(p);
            var w = net.BeginPacket(ReliabilityType.OrderedReliable, p);

            w.Write(global);
            reaction.Write(w);
        }
        private static void Receive(INetEndpoint net, BinaryReader r)
        {
            var station = r.ReadVector3();
            var reaction = r.ReadDef<Reaction>();
            net.Map.Town.CraftingManager.AddOrder(station, reaction);
            if (net is Server)
                Send(net, station, reaction);
        }
    }
}
