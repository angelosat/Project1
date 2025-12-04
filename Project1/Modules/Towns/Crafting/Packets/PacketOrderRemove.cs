using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    class PacketOrderRemove
    {
        static readonly int p;
        static PacketOrderRemove()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(INetEndpoint net, CraftOrder order)
        {
            //var w = net.GetOutgoingStreamOrderedReliable();
            //w.Write(p);
            var w = net.BeginPacketNew(ReliabilityType.OrderedReliable, p);

            w.Write(order.Workstation);
            w.Write(order.ID);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var station = r.ReadIntVec3();
            var orderID = r.ReadInt32();// r.ReadString();
            if (net.Map.Town.CraftingManager.RemoveOrder(station, orderID) is CraftOrder order)
                if (net is Server)
                    Send(net, order);
        }
    }
}
