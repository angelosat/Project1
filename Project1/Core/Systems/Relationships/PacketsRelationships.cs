using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;

namespace Project1.Core.Systems.Relationships
{
    [EnsureStaticCtorCall]
    internal static class PacketsRelationships
    {
        static readonly PacketId _pDeltaApplied = Registry.PacketHandlers.Register(OnDeltaApplied);

        static PacketsRelationships()
        {
            Registry.WorldEventHooksServer.Register<RelationshipDeltaAppliedEvent>(HandleRelationshipDeltaApplied);
        }

        private static void HandleRelationshipDeltaApplied(RelationshipDeltaAppliedEvent e)
        {
            Server.Instance.BeginPacket(_pDeltaApplied)
                .Write(e.Owner)
                .Write(e.Target)
                .Write(e.Delta);
        }

        private static void OnDeltaApplied(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var client = endpoint as Client;
            var actorid = r.ReadEntityRefId();
            var targetid = r.ReadEntityRefId();
            var delta = r.ReadInt32();
            var actor = client.World.Get<Actor>(actorid);
            var target = client.World.Get<Actor>(targetid);
            actor.Relationships.ApplyDelta(target, delta);
        }
    }
}
