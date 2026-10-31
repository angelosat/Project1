using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.World.Rooms;
using Project1.Framework;
using System;

namespace Project1.Core.World.Frontiers;

[EnsureStaticCtorCall]
static class Packets_Frontiers
{
    static readonly PacketId
        _pPlaceAt = Registry.PacketHandlers.Register(ReceivePlaceAt),
        _pExploration = Registry.PacketHandlers.Register(ReceiveExploration);

   

    static Packets_Frontiers()
    {
        Registry.WorldEventHooksServer.Register<WorldExplorationEvent>(SendExploration);
    }

    private static void SendExploration(WorldExplorationEvent e)
    {
        Server.Instance.BeginPacket(_pExploration)
            .Write(e.Actor.RefId)
            .Write(e.Frontier.Def);
    }

    private static void ReceiveExploration(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var world = (StaticWorld)endpoint.World;
        var actor = world.Get<Actor>(r.ReadEntityRefId());
        var frontier = world.Space.Get(r.ReadDef<FrontierDef>());
        var manager = (FrontierManager)world.Space;
        manager.Explore(frontier, actor);
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
