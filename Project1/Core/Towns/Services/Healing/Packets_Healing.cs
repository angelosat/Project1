using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Systems.Magic;
using Project1.Framework;

namespace Project1.Core.Towns.Services.Healing;

[EnsureStaticCtorCall]
internal static class Packets_Healing
{
    static readonly PacketId
        _pCreate = Registry.PacketHandlers.Register(ReceiveCreate);
        //_pSync = Registry.PacketHandlers.Register(ReceiveSync);
    static Packets_Healing()
    {
        //Registry.MapEventHooksServer.Register<HealingRequestUpdatedEvent>(HandleRequestUpdated);
        Registry.MapEventHooksServer.Register<HealingRequestCreatedEvent>(HandleRequestCreated);
    }

    private static void HandleRequestCreated(HealingRequestCreatedEvent e)
    {
        SendRequestCreated(Server.Instance, e.Target, e.Spell);
    }
    private static void SendRequestCreated(NetEndpoint endpoint, Actor target, SpellDef spell)
    {
        var w = endpoint.BeginPacket(_pCreate)
            .Write(target.RefId)
            .Write(spell);
    }
    private static void ReceiveCreate(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var target = endpoint.World.Get<Actor>(r.ReadEntityRefId());
        var spell = r.ReadDef<SpellDef>();
        var manager = target.Map.Town.Spells;
        manager.Request(target, spell);
    }
    //private static void HandleRequestUpdated(HealingRequestUpdatedEvent e)
    //{
    //    SendRequestUpdated(Server.Instance, e.Request);
    //}

    //private static void SendRequestUpdated(NetEndpoint endpoint, SpellRequest request)
    //{
    //    var w = endpoint.BeginPacket(_pSync);
    //    w.Write(request.TargetId);
    //    request.Write(w);
    //}

    //private static void ReceiveSync(NetEndpoint endpoint, Packet packet)
    //{
    //    var r = packet.PacketReader;
    //    var target = endpoint.World.Get<Actor>(r.ReadEntityRefId());
    //    var manager = target.Map.Town.SpellManager;
    //    var req = manager.GetRequestbyTargetOrDefault(target);
    //    req.Read(r);
    //    //throw new System.Exception();
    //    //req.ReadExtra(r);
    //}
}
