using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketPlayerSetItemOwner
    {
        static readonly int PacketIDPlayerSetItemOwner;
        static PacketPlayerSetItemOwner()
        {
            PacketIDPlayerSetItemOwner = Registry.PacketHandlers.Register(Receive);
        }
        static public void Send(INetEndpoint net, int itemID, int ownerID)
        {
            //var stream = net.GetOutgoingStreamOrderedReliable();
            //stream.Write(PacketIDPlayerSetItemOwner);
            var stream = net.BeginPacketNew(ReliabilityType.OrderedReliable, PacketIDPlayerSetItemOwner);
            stream.Write(itemID);
            stream.Write(ownerID);
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var itemID = r.ReadInt32();
            var ownerID = r.ReadInt32();
            var item = net.GetNetworkEntity(itemID);
            
            item.SetOwner(ownerID);
         
            if (net is Server)
                Send(net, itemID, ownerID);
        }
    }
}
