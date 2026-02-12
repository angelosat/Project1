using System;
using Microsoft.Xna.Framework;
using Project1.Framework;
using Project1.Core.Entities.Actors;
using Project1.Framework.Events;
using Project1.Core.Networking;

namespace Project1.Core.Networking.Packets
{
    [EnsureStaticCtorCall]
    internal class PacketActorThrowHauled
    {
        static readonly int pType;
        static PacketActorThrowHauled()
        {
            pType = Registry.PacketHandlers.Register(Receive);
        }
        static public void Send(Actor actor, Vector3 velocity, int amount = -1)
        {
            var server = actor.Net as Server;
            server.BeginPacket(pType)
                .Write(actor.RefId)
                .Write(velocity)
                .Write(amount);
            actor.Inventory.Throw(velocity, amount);
        }
        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            if (endpoint is Server)
                throw new Exception();
            var r = packet.PacketReader;
            var actor = endpoint.World.GetEntity(r.ReadInt32());
            var velocity = r.ReadVector3();
            var amount = r.ReadInt32();
            actor.Inventory.Throw(velocity, amount);
        }
    }
}
