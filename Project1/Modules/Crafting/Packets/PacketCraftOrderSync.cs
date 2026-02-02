using Project1.Framework.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    class PacketCraftOrderSync
    {
        static readonly int p;
        static PacketCraftOrderSync()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }

        internal static void Send(CraftOrder order, Stockpile input, Stockpile output)
        {
            var net = order.Map.Net;
            var w = net.BeginPacket(p);
            w.Write(order.ID);
            w.Write(input?.ID ?? -1);
            w.Write(output?.ID ?? -1);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var station = r.ReadIntVec3();
            var index = r.ReadInt32();
            var order = net.Map.Town.CraftingManager.GetOrder(index);
            var manager = net.Map.Town.ZoneManager;
            var input = r.ReadInt32() is int inputID && inputID == -1 ? null : manager.GetZone<Stockpile>(inputID);
            var output = r.ReadInt32() is int outputID && outputID == -1 ? null : manager.GetZone<Stockpile>(outputID);
            order.Input = input;
            order.Output = output;
            net.Map.EventOccured(Components.Message.Types.OrderParametersChanged, order);
            if (net is Server)
                Send(order, input, output);
        }
    }
}
