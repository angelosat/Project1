using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;
using System;

namespace Project1.Core.Networking.Entities
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
            var actorId = packet.PacketReader.ReadEntityRefId();
            var targetId = packet.PacketReader.ReadEntityRefId();
            var actor = endpoint.World.Get(actorId);
            var target = endpoint.World.Get(targetId);
            actor.Inventory.Drop(target);
        }
    }
}
