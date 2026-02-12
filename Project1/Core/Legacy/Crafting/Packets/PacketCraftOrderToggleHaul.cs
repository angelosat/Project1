using Project1.Core.Components;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Networking;
using Project1.Core.Networking;
using Project1.Framework.Events;

namespace Project1.Core.Legacy.Crafting.Packets
{
    class PacketCraftOrderToggleHaul
    {
        static int p;
        static internal void Init()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }

        internal static void Send(CraftOrderOld order, bool value)
        {
            var net = order.Map.Net;
            var bench = order.Workstation;
            var w = net.BeginPacket(p);

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
            net.Map.EventOccured(Message.Types.OrderParametersChanged, order);
            if (net is Server)
                Send(order, order.HaulOnFinish);
        }
    }
}
