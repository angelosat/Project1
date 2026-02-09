using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core;
using Project1.Core.Net;
using Project1.Core.Entities;

namespace Project1.Core.Net.Packets
{
    [EnsureStaticCtorCall]
    public static class PacketSlotAssign
    {
        static readonly int pType;
        static PacketSlotAssign()
        {
            pType = Registry.PacketHandlers.Register(Receive);
        }

        public static void Send(Entity owner, int slotId, Entity item)
        {
            var server = owner.Net as Server;
            server.BeginPacket(pType)
                .Write(owner.RefId)
                .Write(slotId)
                .Write(item?.RefId ?? -1);
        }
        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var ownerId = r.ReadInt32();
            var owner = client.World.GetEntity<Actor>(ownerId);
            var slotId = r.ReadInt32();
            var itemId = r.ReadInt32();
            var item = itemId > 0 ? client.World.GetEntity(itemId) : null;
            owner.GetSlot(slotId).Assign(item);
        }
    }
}
