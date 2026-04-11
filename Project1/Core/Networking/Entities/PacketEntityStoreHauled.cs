using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;
using System;

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
            var actorId = packet.PacketReader.ReadEntityRefId();
            var actor = endpoint.World.Get(actorId);
            actor.Inventory.StoreHauled();
        }
    }
}
