using Project1.Framework.Net;

namespace Start_a_Town_
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
