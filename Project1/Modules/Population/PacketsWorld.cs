using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class PacketsWorld
    {
        static readonly int _pInhabitantGenerated;
        static PacketsWorld()
        {
            _pInhabitantGenerated = Registry.PacketHandlers.Register(OnInhabitantGenerated);

            Registry.WorldEventHooksServer.Register<WorldInhabitantGeneratedEvent>(SendWorldInhabitantGenerated);
        }

        private static void SendWorldInhabitantGenerated(WorldInhabitantGeneratedEvent e)
        {
            Server.Instance.BeginPacket(_pInhabitantGenerated)
                .Write(e.Actor.RefId)
                .Write(e.WorldPosition);
        }

        private static void OnInhabitantGenerated(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.GetEntity<Actor>(r.ReadInt32());
            var worldPos = WorldSpacePosition.ReadFrom(r);
            actor.World.PlaceAt(actor, worldPos);
        }
    }
}
