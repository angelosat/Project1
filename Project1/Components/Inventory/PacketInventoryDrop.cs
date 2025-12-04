using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketInventoryDrop
    {
        static readonly int p;
        static PacketInventoryDrop()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(INetEndpoint net, GameObject actor, GameObject item, int amount)
        {
            //var stream = net.GetOutgoingStreamOrderedReliable();
            //stream.Write(p);
            var stream = actor.Net.BeginPacketNew(ReliabilityType.OrderedReliable, p);
            stream.Write(actor.RefId);
            stream.Write(item.RefId);
            stream.Write(amount);
        }
        static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var actorID = r.ReadInt32();
            var itemID = r.ReadInt32();
            var amount = r.ReadInt32();
            var item = net.GetNetworkEntity(itemID);
            var actor = net.GetNetworkEntity(actorID);
            actor.Inventory.Drop(item, amount); // TODO: this happens immediately when the game is paused. maybe create an interaction with a 1 frame duration?
            if (amount == item.StackSize)
                NpcComponent.RemovePossesion(actor, item);
            if (net is Server)
                Send(net, actor, item, amount);
        }
    }
}
