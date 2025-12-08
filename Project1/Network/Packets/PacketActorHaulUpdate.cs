using Start_a_Town_.Net;

namespace Start_a_Town_
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
            var actor = client.World.GetEntity(r.ReadInt32());
            var itemId = r.ReadInt32();
            var item = itemId > 0 ? client.World.GetEntity(itemId) : null;
            var amount = r.ReadInt32();
            actor.Inventory.HaulSlot.Object = item;
        }
    }
}
