using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketCraftOrderToggleHaul
    {
        static int p;
        static internal void Init()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }

        internal static void Send(CraftOrder order, bool value)
        {
            var net = order.Map.Net;
            //var w = net.GetOutgoingStreamOrderedReliable();
            
            //w.Write(p);
            var bench = order.Workstation;
            var w = net.BeginPacketNew(ReliabilityType.OrderedReliable, p);

            w.Write(bench);
            w.Write(order.ID);
            w.Write(value);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var station = r.ReadIntVec3();
            var id = r.ReadInt32();
            var order = net.Map.Town.CraftingManager.GetOrder(station, id);
            order.HaulOnFinish = r.ReadBoolean();
            net.Map.EventOccured(Components.Message.Types.OrderParametersChanged, order);
            if (net is Server)
                Send(order, order.HaulOnFinish);
        }
    }
}
