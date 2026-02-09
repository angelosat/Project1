using Project1.Core.Base;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Net;
using Project1.Core.Helpers;
using Project1.Core.Net;


namespace Project1.Core.Crafting.Packets
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
            var w = net.BeginPacketNew(ReliabilityType.OrderedReliable, p);

            w.Write(order.Workstation);
            w.Write(order.ID);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var station = r.ReadIntVec3();
            var orderID = r.ReadInt32();
            if (net.Map.Town.CraftingManager.RemoveOrder(station, orderID) is CraftOrder order)
                if (net is Server)
                    Send(net, order);
        }
    }
}
