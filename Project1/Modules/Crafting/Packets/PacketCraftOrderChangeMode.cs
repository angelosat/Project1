using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketCraftOrderChangeMode
    {
        static int p;
        static internal void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }

        internal static void Send(CraftOrder order, int value)
        {
            var net = order.Map.Net;
            var w = net.GetOutgoingStream();
            //var bench = order.Workstation;
            w.Write(p);
            w.Write(order.Workstation);
            w.Write(order.ID);// GetUniqueLoadID());
            w.Write(value);
        }
        private static void Receive(INetPeer net, BinaryReader r)
        {
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
