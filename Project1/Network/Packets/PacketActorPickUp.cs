using Start_a_Town_.Net;
using System;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal class PacketActorPickUp
    {
        static readonly int pType;
        static PacketActorPickUp()
        {
            pType = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(Entity actor, Entity target)
        {
            var server = actor.Net as Server;
            server.BeginPacket(pType)
                .Write(actor.RefId)
                .Write(target.RefId);
            //actor.Inventory.HaulSlot.Object = target;
            actor.Inventory.Haul(target);
        }
        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            if (endpoint.IsServer)
                throw new Exception();
            var actorId = packet.PacketReader.ReadInt32();
            var targetId = packet.PacketReader.ReadInt32();
            var actor = endpoint.World.GetEntity(actorId);
            var target = endpoint.World.GetEntity(targetId);
            //actor.Inventory.HaulSlot.Object = target;
            actor.Inventory.Haul(target);
        }
    }
}
