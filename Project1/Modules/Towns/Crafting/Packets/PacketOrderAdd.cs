using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketOrderAdd
    {
        static int p;
        static internal void Init()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }

        internal static void Send(NetEndpoint net, Vector3 global, Reaction reaction)
        {
            var w = net.BeginPacketOld(p);

            w.Write(global);
            reaction.Write(w);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var station = r.ReadVector3();
            var reaction = r.ReadDef<Reaction>();
            net.Map.Town.CraftingManager.AddOrder(station, reaction);
            if (net is Server)
                Send(net, station, reaction);
        }
    }
}
