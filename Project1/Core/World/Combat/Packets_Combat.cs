using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.World;
using Project1.Core.World.WorldAreas;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project1.Core.World.Combat;

[EnsureStaticCtorCall]
static class Packets_Combat
{
    static PacketId
        _pCombatBegin = Registry.PacketHandlers.Register(ReceiveCombatBegin),
        _pCombatEnd = Registry.PacketHandlers.Register(ReceiveCombatEnd);

    static Packets_Combat()
    {
        Registry.WorldEventHooksServer.Register<CombatBeginEvent>(SendCombatBegin);
        Registry.WorldEventHooksServer.Register<CombatEndEvent>(SendCombatEnd);
    }

    private static void SendCombatBegin(CombatBeginEvent e)
    {
        Server.Instance.BeginPacket(_pCombatBegin)
            .Write(e.Id)
            .Write(e.Attacker.RefId)
            .Write(e.Defender.RefId);
    }

    private static void SendCombatEnd(CombatEndEvent e)
    {
        Server.Instance.BeginPacket(_pCombatEnd)
            .Write(e.Id);
    }

    private static void ReceiveCombatBegin(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var combatId = r.ReadInt32();
        var attackerid = r.ReadEntityRefId();
        var defenderid = r.ReadEntityRefId();
        var world = (StaticWorld)endpoint.World;
        var attacker = world.Get<Actor>(attackerid);
        var defender = world.Get<Actor>(defenderid);
        world.Combat.RegisterCombat(combatId, attacker, defender);
    }

    private static void ReceiveCombatEnd(NetEndpoint endpoint, Packet packet)
    {
        throw new NotImplementedException();
    }
}
