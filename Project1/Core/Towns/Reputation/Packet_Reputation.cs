using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core.Towns.Reputation;

[EnsureStaticCtorCall]
internal static class Packet_Reputation
{
    internal static readonly PacketId
        _pReputationDelta = Registry.PacketHandlers.Register(ReceiveReputationDelta);

    static Packet_Reputation()
    {
        Registry.MapEventHooksServer.Register<ReputationDeltaAppliedEvent>(HandleReputationDeltaApplied);
    }

    private static void HandleReputationDeltaApplied(ReputationDeltaAppliedEvent e)
    {
        SendReputationDelta(Server.Instance, e.Map, e.Actor, e.Delta);
    }

    private static void SendReputationDelta(NetEndpoint endpoint, MapBase map, Actor actor, int delta)
    {
        endpoint.BeginPacket(_pReputationDelta)
            .Write(map.ID)
            .Write(actor.RefId)
            .Write(delta);
    }

    private static void ReceiveReputationDelta(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var map = endpoint.World.Get(r.ReadMapId());
        var actor = endpoint.World.Get<Actor>(r.ReadEntityRefId());
        var delta = r.ReadInt32();
        map.Town.Reputation.ApplyDelta(actor, delta);
    }
}
