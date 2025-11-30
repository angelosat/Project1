using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketPlayerSetItemOwner
    {
        static readonly int PacketIDPlayerSetItemOwner;
        static PacketPlayerSetItemOwner()
        {
            PacketIDPlayerSetItemOwner = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(INetPeer net, int itemID, int ownerID)
        {
            var stream = net.GetOutgoingStream();
            stream.Write(PacketIDPlayerSetItemOwner);
            stream.Write(itemID);
            stream.Write(ownerID);
        }
        static public void Receive(INetPeer net, BinaryReader r)
        {
            var itemID = r.ReadInt32();
            var ownerID = r.ReadInt32();
            var item = net.GetNetworkEntity(itemID);
            
            item.SetOwner(ownerID);
         
            if (net is Server)
                Send(net, itemID, ownerID);
        }
    }
}
