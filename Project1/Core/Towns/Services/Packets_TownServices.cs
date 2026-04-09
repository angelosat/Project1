using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.Systems.Magic;
using Project1.Core.Towns.Healing;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project1.Core.Towns.Services;

[EnsureStaticCtorCall]
internal class Packets_TownServices
{
    static readonly PacketId
        //_pCreate = Registry.PacketHandlers.Register(ReceiveCreate),
        _pSync = Registry.PacketHandlers.Register(ReceiveSync);

    static Packets_TownServices()
    {
        Registry.MapEventHooksServer.Register<TownServiceRequestUpdatedEvent>(HandleRequestUpdated);
        //Registry.MapEventHooksServer.Register<HealingRequestCreatedEvent>(HandleRequestCreated);
    }

    //private static void HandleRequestCreated(HealingRequestCreatedEvent e)
    //{
    //    SendRequestCreated(Server.Instance, e.Target, e.Spell);
    //}
    //private static void SendRequestCreated(NetEndpoint endpoint, Actor target, SpellDef spell)
    //{
    //    var w = endpoint.BeginPacket(_pCreate)
    //        .Write(target.RefId)
    //        .Write(spell);
    //}
    //private static void ReceiveCreate(NetEndpoint endpoint, Packet packet)
    //{
    //    var r = packet.PacketReader;
    //    var target = endpoint.World.Get<Actor>(r.ReadEntityRefId());
    //    var spell = r.ReadDef<SpellDef>();
    //    var manager = target.Map.Town.SpellManager;
    //    manager.Request(target, spell);
    //}
    private static void HandleRequestUpdated(TownServiceRequestUpdatedEvent e)
    {
        SendRequestUpdated(Server.Instance, e.Map, e.Request);
    }

    private static void SendRequestUpdated(NetEndpoint endpoint, MapBase map, TownServiceRequest request)
    {
        var w = endpoint.BeginPacket(_pSync)
            .Write(map.ID)
            .Write(request.Id);
        request.Write(w);
    }

    private static void ReceiveSync(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        //var target = endpoint.World.Get<Actor>(r.ReadEntityRefId());
        //var manager = target.Map.Town.SpellManager;
        //var req = manager.GetRequestbyTargetOrDefault(target);
        //req.ReadExtra(r);
        var map = endpoint.World.Get(r.ReadMapId());
        var manager = map.Town.ServiceRequests;
        var request = manager.Get(r.ReadUInt64());
        request.Read(r);
    }
}
