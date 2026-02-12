using Project1.Core.Components;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Networking;
using Project1.Core.Networking;
using Project1.Framework.Events;

namespace Project1.Core.Legacy.Crafting.Packets
{
    class PacketCraftOrderChangeMode
    {
        static int p;
        static internal void Init()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }

        internal static void Send(CraftOrderOld order, int value)
        {
            var net = order.Map.Net;
            var w = net.BeginPacketNew(ReliabilityType.OrderedReliable, p);
            w.Write(order.Workstation);
            w.Write(order.ID);
            w.Write(value);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var station = r.ReadIntVec3();
            var index = r.ReadInt32();
            var bench = net.Map.Town.CraftingManager.GetWorkstation(station);
            var order = bench.GetOrder(index);
            order.FinishMode = CraftOrderFinishMode.GetMode(r.ReadInt32());
            net.Map.EventOccured(Message.Types.OrderParametersChanged, order);
            if (net is Server)
                Send(order, (int)order.FinishMode.Mode);
        }
    }
}
