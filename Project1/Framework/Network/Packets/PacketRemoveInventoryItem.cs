using Project1.Framework.Base;
using Project1.Framework.Entities;
using Project1.Framework.Entities.Actors;
using Start_a_Town_;

namespace Project1.Framework.Net.Packets
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
