using Project1.Core.World.WorldAreas;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Net;
using Project1.Core;
using Project1.Core.Net;

namespace Project1.Core.Population
{
    [EnsureStaticCtorCall]
    internal static class PacketsWorld
    {
        static readonly int _pInhabitantGenerated;
        static PacketsWorld()
        {
            _pInhabitantGenerated = Registry.PacketHandlers.Register(OnInhabitantGenerated);

            Registry.WorldEventHooksServer.Register<InhabitantPlacedInWorldEvent>(SendWorldInhabitantGenerated);
        }

        private static void SendWorldInhabitantGenerated(InhabitantPlacedInWorldEvent e)
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
            client.World.PlaceAt(actor, worldPos);
        }
    }
}
