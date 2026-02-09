using System;
using Project1.Framework;
using Project1.Core.Base;
using Project1.Core.Entities;
using Project1.Core.Net;

namespace Project1.Core.Networking.Entities
{
    [EnsureStaticCtorCall]
    internal class PacketEntityStoreHauled
    {
        static readonly int pType;
        static PacketEntityStoreHauled()
        {
            pType = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(Entity actor)
        {
            var server = actor.Net as Server;
            server.BeginPacket(pType)
                .Write(actor.RefId);
            actor.Inventory.StoreHauled();

        }
        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            if (endpoint.IsServer)
                throw new Exception();
            var actorId = packet.PacketReader.ReadInt32();
            var actor = endpoint.World.GetEntity(actorId);
            actor.Inventory.StoreHauled();
        }
    }
}
