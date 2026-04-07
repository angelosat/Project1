using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Framework;
using System;

namespace Project1.Core.World.WorldAreas;

[EnsureStaticCtorCall]
static class Packets_Frontiers
{
    static readonly int _pPlaceAt;
    static Packets_Frontiers()
    {
        _pPlaceAt = Registry.PacketHandlers.Register(ReceivePlaceAt);
    }

    static internal void SendPlaceAt(Actor actor, float pos)
    {
        (actor.Net as Server).BeginPacket(_pPlaceAt)
            .Write(actor.RefId)
            .Write(pos);
    }
    private static void ReceivePlaceAt(NetEndpoint endpoint, Packet packet)
    {
        var client = endpoint as Client ?? throw new Exception();
        var r = packet.PacketReader;
        var actor = client.World.Get<Actor>(r.ReadInt32());
        var pos = r.ReadSingle();
        ((actor.World as StaticWorld).Space as FrontierManager).PlaceAt(actor, pos);
    }
}
