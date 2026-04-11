using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;

namespace Project1.Core.Networking.Inventory
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
            var actorID = r.ReadEntityRefId();
            var itemID = r.ReadEntityRefId();
            var item = net.World.Get(itemID);
            var actor = net.World.Get(actorID) as Actor;
            actor.Equip(item);
            if (net is Server)
                Send(net, actorID, itemID);
        }
    }
}
