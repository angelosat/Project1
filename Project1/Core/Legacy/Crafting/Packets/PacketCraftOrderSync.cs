using Project1.Framework;
using Project1.Core.Components;
using Project1.Core.Networking;
using Project1.Framework.Events;
using Project1.Core.Networking;
using Project1.Core.Towns.Stockpiles;

namespace Project1.Core.Legacy.Crafting.Packets
{
    [EnsureStaticCtorCall]
    class PacketCraftOrderSync
    {
        static readonly int p;
        static PacketCraftOrderSync()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }

        internal static void Send(CraftOrderOld order, Stockpile input, Stockpile output)
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
            net.Map.EventOccured(Message.Types.OrderParametersChanged, order);
            if (net is Server)
                Send(order, input, output);
        }
    }
}
