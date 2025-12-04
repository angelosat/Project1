using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketInventoryEquip
    {
        static readonly int p;
        static PacketInventoryEquip()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        static public void Send(INetEndpoint net, int actorID, int itemID)
        {
            //var stream = net.GetOutgoingStreamOrderedReliable();
            //stream.Write(p);
            var stream = net.BeginPacketNew(ReliabilityType.OrderedReliable, p);
            stream.Write(actorID);
            stream.Write(itemID);
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var actorID = r.ReadInt32();
            var itemID = r.ReadInt32();
            var item = net.GetNetworkEntity(itemID);
            var actor = net.GetNetworkEntity(actorID) as Actor;
            actor.Equip(item);
            if (net is Server)
                Send(net, actorID, itemID);
        }
    }
}
