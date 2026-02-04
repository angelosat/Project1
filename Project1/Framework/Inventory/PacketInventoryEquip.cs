using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;
using Project1.Framework.Net;
using Start_a_Town_;

namespace Project1.Framework.Inventory
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
            var stream = net.BeginPacketNew(ReliabilityType.OrderedReliable, p);
            stream.Write(actorID);
            stream.Write(itemID);
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var actorID = r.ReadInt32();
            var itemID = r.ReadInt32();
            var item = net.World.GetEntity(itemID);
            var actor = net.World.GetEntity(actorID) as Actor;
            actor.Equip(item);
            if (net is Server)
                Send(net, actorID, itemID);
        }
    }
}
