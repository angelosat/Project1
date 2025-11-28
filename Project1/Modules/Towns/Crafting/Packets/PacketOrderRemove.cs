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
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetPeer net, CraftOrder order)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(order.Workstation);
            w.Write(order.ID);
        }
        private static void Receive(INetPeer net, BinaryReader r)
        {
            var station = r.ReadIntVec3();
            var orderID = r.ReadInt32();// r.ReadString();
            if (net.Map.Town.CraftingManager.RemoveOrder(station, orderID) is CraftOrder order)
                if (net is Server)
                    Send(net, order);
        }
    }
}
