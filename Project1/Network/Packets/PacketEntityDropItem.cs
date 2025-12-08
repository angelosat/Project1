using Start_a_Town_;
using Start_a_Town_.Net;
using System;

namespace Project1.Network.Packets
{
    [EnsureStaticCtorCall]
    internal class PacketEntityDropItem
    {
        static readonly int pType;
        static PacketEntityDropItem()
        {
            pType = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(Entity actor, Entity target)
        {
            var server = actor.Net as Server;
            server.BeginPacket(pType)
                .Write(actor.RefId)
                .Write(target.RefId);
            actor.Inventory.Drop(target);
        }
        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            if (endpoint.IsServer)
                throw new Exception();
            var actorId = packet.PacketReader.ReadInt32();
            var targetId = packet.PacketReader.ReadInt32();
            var actor = endpoint.World.GetEntity(actorId);
            var target = endpoint.World.GetEntity(targetId);
            actor.Inventory.Drop(target);
        }
    }
}
