using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Framework;
using System;

namespace Project1.Core.World.WorldAreas;

[EnsureStaticCtorCall]
internal static class Packets_Encounters
{
    static readonly PacketId 
        _encounterBegin = Registry.PacketHandlers.Register(ReceiveEncounterBegin),
        _encounterEnd = Registry.PacketHandlers.Register(ReceiveEncounterEnd);

    static Packets_Encounters()
    {
        Registry.WorldEventHooksServer.Register<EncounterBeginEvent>(SendEncounterBegin);
        Registry.WorldEventHooksServer.Register<EncounterEndEvent>(SendEncounterEnd);
    }

    private static void SendEncounterBegin(EncounterBeginEvent e)
    {
        Server.Instance
            .BeginPacket(_encounterBegin)
            .Write(e.Entity.RefId)
            .Write(e.Encounter.Def);
    }

    private static void SendEncounterEnd(EncounterEndEvent e)
    {
        Server.Instance
            .BeginPacket(_encounterEnd)
            .Write(e.Entity.RefId);
    }

    private static void ReceiveEncounterBegin(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var actorid = r.ReadEntityRefId();
        var world = (StaticWorld)endpoint.World;
        var actor = world.Get<Actor>(actorid);
        var encounterDef = r.ReadDef<EncounterDef>();
        var frontiers = (FrontierManager)world.Space;
        frontiers.BeginEncounter(actor, encounterDef);
    }

    private static void ReceiveEncounterEnd(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var actorid = r.ReadEntityRefId();
        var world = (StaticWorld)endpoint.World;
        var actor = world.Get<Actor>(actorid);
        var frontiers = (FrontierManager)world.Space;
        frontiers.EndEncounter(actor);
    }
}
