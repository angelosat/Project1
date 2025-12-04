using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketCraftOrderChangeMode
    {
        static int p;
        static internal void Init()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }

        internal static void Send(CraftOrder order, int value)
        {
            var net = order.Map.Net;
            //var w = net.GetOutgoingStreamOrderedReliable();
            ////var bench = order.Workstation;
            //w.Write(p);
            var w = net.BeginPacketNew(ReliabilityType.OrderedReliable, p);
            w.Write(order.Workstation);
            w.Write(order.ID);// GetUniqueLoadID());
            w.Write(value);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var station = r.ReadIntVec3();//.ReadVector3();
            var index = r.ReadInt32();// r.ReadString();
            var bench = net.Map.Town.CraftingManager.GetWorkstation(station);
            var order = bench.GetOrder(index);
            order.FinishMode = CraftOrderFinishMode.GetMode(r.ReadInt32());
            net.Map.EventOccured(Components.Message.Types.OrderParametersChanged, order);
            if (net is Server)
                Send(order, (int)order.FinishMode.Mode);
        }
    }
}
