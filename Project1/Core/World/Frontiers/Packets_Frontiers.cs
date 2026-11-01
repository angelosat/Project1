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
        _pActorEnterPoI = Registry.PacketHandlers.Register(ReceiveActorEnterPoI),
        _pActorExitPoI = Registry.PacketHandlers.Register(ReceiveActorExitPoI),

        _pPlaceAt = Registry.PacketHandlers.Register(ReceivePlaceAt),
        _pExploration = Registry.PacketHandlers.Register(ReceiveExploration);

    

    static Packets_Frontiers()
    {
        Registry.WorldEventHooksServer.Register<WorldExplorationEvent>(SendExploration);
        Registry.WorldEventHooksServer.Register<ActorEnterPoIEvent>(SendActorEnterPoI);
        Registry.WorldEventHooksServer.Register<ActorExitPoIEvent>(SendActorExitPoI);
    }

    private static void SendActorEnterPoI(ActorEnterPoIEvent e)
    {
        Server.Instance.BeginPacket(_pActorEnterPoI)
            .Write(e.Actor.RefId)
            .Write(e.Frontier.Def)
            .Write(e.Frontier.PoIs.IndexOf(e.PoI));
    }

    private static void SendActorExitPoI(ActorExitPoIEvent e)
    {
        Server.Instance.BeginPacket(_pActorExitPoI)
            .Write(e.Actor.RefId);
    }

    private static void ReceiveActorEnterPoI(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var manager = (FrontierManager)((StaticWorld)endpoint.World).Space;
        var actor = endpoint.World.Get<Actor>(r.ReadEntityRefId());
        var frontierDef = r.ReadDef<FrontierDef>();
        var poiIndex = r.ReadInt32();
        manager.ActorEnterPoI(actor, frontierDef, poiIndex);
    }

    private static void ReceiveActorExitPoI(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var manager = (FrontierManager)((StaticWorld)endpoint.World).Space;
        var actor = endpoint.World.Get<Actor>(r.ReadEntityRefId());
        manager.ActorPoIExit(actor);
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
