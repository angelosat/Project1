using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketInventoryEquip
    {
        static readonly int p;
        static PacketInventoryEquip()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(INetwork net, int actorID, int itemID)
        {
            var stream = net.GetOutgoingStream();
            stream.Write(p);
            stream.Write(actorID);
            stream.Write(itemID);
        }
        static public void Receive(INetwork net, BinaryReader r)
        {
            var actorID = r.ReadInt32();
            var itemID = r.ReadInt32();
            var item = net.GetNetworkObject(itemID);
            var actor = net.GetNetworkObject(actorID) as Actor;
            actor.Equip(item);
            if (net is Server)
                Send(net, actorID, itemID);
        }
    }
}
