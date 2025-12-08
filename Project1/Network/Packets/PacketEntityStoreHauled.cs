using Start_a_Town_.Net;
using System;

namespace Start_a_Town_
{
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
            //actor.Inventory.Drop(target);
            actor.Inventory.StoreHauled();

        }
        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            if (endpoint.IsServer)
                throw new Exception();
            var actorId = packet.PacketReader.ReadInt32();
            var actor = endpoint.World.GetEntity(actorId);
            //actor.Inventory.Drop(target);
            actor.Inventory.StoreHauled();
        }
    }
}
