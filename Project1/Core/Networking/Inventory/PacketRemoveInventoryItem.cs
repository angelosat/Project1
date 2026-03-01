using Project1.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities;

namespace Project1.Core.Networking.Inventory
{
    [EnsureStaticCtorCall]
    internal class PacketRemoveInventoryItem
    {
        static readonly int _packetTypeId;
        static PacketRemoveInventoryItem()
        {
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }
        public static void Send(Actor actor, Entity item)
        {
            var server = actor.Net as Server;
            server.BeginPacket(_packetTypeId)
                .Write(actor.RefId)
                .Write(item.RefId);
        }

        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.GetEntity(r.ReadInt32());
            var item = client.World.GetEntity(r.ReadInt32());
            actor.Inventory.Remove(item);
        }
    }
}
