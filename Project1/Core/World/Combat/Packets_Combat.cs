//using Project1.Core.Networking;
//using Project1.Core.Simulation;
//using Project1.Framework;

//namespace Project1.Core.World.Combat;

//[EnsureStaticCtorCall]
//static class Packets_Combat
//{
//    static PacketId
//        //////_pCombatBegin = Registry.PacketHandlers.Register(ReceiveCombatBegin),
//        //_pCombatEnd = Registry.PacketHandlers.Register(ReceiveCombatEnd),
//        _pCombatEvent = Registry.PacketHandlers.Register(ReceiveCombatEvent);

//    static Packets_Combat()
//    {
//        //////Registry.WorldEventHooksServer.Register<CombatBeginEvent>(SendCombatBegin);
//        //Registry.WorldEventHooksServer.Register<CombatEndEvent>(SendCombatEnd);
//        Registry.WorldEventHooksServer.Register<WorldCombatEvent>(SendCombatEvent);
//    }

//    private static void SendCombatEvent(WorldCombatEvent e)
//    {
//        var w = Server.Instance.BeginPacket(_pCombatEvent);
//        e.Write(w);
//    }

//    private static void ReceiveCombatEvent(NetEndpoint endpoint, Packet packet)
//    {
//        var r = packet.PacketReader;
//        var e = WorldCombatEvent.Create(r);
//        e.World = (StaticWorld)endpoint.World;
//        CombatSystem.IncrementTrackingStats(e);
//    }

//    //private static void SendCombatEnd(CombatEndEvent e)
//    //{
//    //    Server.Instance.BeginPacket(_pCombatEnd)
//    //        .Write(e.CombatSessionId);
//    //}

//    //private static void ReceiveCombatEnd(NetEndpoint endpoint, Packet packet)
//    //{
//    //    var r = packet.PacketReader;
//    //    var combatId = r.ReadInt32();
//    //    var world = (StaticWorld)endpoint.World;
//    //    world.Combat.UnregisterCombat(combatId);
//    //}

//    //private static void SendCombatBegin(CombatBeginEvent e)
//    //{
//    //    Server.Instance.BeginPacket(_pCombatBegin)
//    //        .Write(e.CombatSessionId)
//    //        .Write(e.Attacker.RefId)
//    //        .Write(e.Defender.RefId);
//    //}

//    //private static void ReceiveCombatBegin(NetEndpoint endpoint, Packet packet)
//    //{
//    //    var r = packet.PacketReader;
//    //    var combatId = r.ReadInt32();
//    //    var attackerid = r.ReadEntityRefId();
//    //    var defenderid = r.ReadEntityRefId();
//    //    var world = (StaticWorld)endpoint.World;
//    //    var attacker = world.Resolve<Actor>(attackerid);
//    //    var defender = world.Resolve<Actor>(defenderid);
//    //    world.Combat.RegisterCombat(combatId, attacker, defender);
//    //}
//}
