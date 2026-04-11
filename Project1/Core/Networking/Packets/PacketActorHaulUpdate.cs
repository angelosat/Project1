using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;

namespace Project1.Core.Networking.Packets
{
    [EnsureStaticCtorCall]
    public static class PacketActorHaulUpdate
    {
        static readonly int pType;
        static PacketActorHaulUpdate()
        {
            pType = Registry.PacketHandlers.Register(Receive);
        }

        public static void Send(Actor actor, Entity newItem, int amount = -1)
        {
            var server = actor.Net as Server;
            server.BeginPacket(pType)
                .Write(actor.RefId)
                .Write(newItem?.RefId ?? -1)
                .Write(amount);
        }
        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.Get(r.ReadEntityRefId());
            var itemId = r.ReadEntityRefId();
            var item = itemId > 0 ? client.World.Get(itemId) : null;
            var amount = r.ReadInt32();
            actor.Inventory.HaulSlot.Assign(item);
        }
    }
}
