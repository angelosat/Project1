using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Systems.Magic;
using Project1.Framework;

namespace Project1.Core.Towns.Services.Spells;

[EnsureStaticCtorCall]
internal static class Packets_Healing
{
    static readonly PacketId
        _pPlayerSpellToggled = Registry.PacketHandlers.Register(ReceivePlayerSpellToggled),
        _pCreate = Registry.PacketHandlers.Register(ReceiveCreate);

    static Packets_Healing()
    {
        Registry.MapEventHooksServer.Register<HealingRequestCreatedEvent>(HandleRequestCreated);

        Registry.PlayerInputEventHooks.Register<PlayerTownSpellToggledEvent>(HandlePlayerSpellToggle);
    }

    private static void HandlePlayerSpellToggle(PlayerTownSpellToggledEvent e)
    {
        if (Ingame.Net.IsServer)
            e.Map.Town.Spells.ToggleSpell(e.Spell);
        Ingame.Net.BeginPacketImmediate(_pPlayerSpellToggled)
            .Write(e.Map.ID)
            .Write(e.Spell);
    }
    private static void ReceivePlayerSpellToggled(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var map = endpoint.World.Get(r.ReadMapId());
        var spell = r.ReadDef<SpellDef>();
        map.Town.Spells.ToggleSpell(spell);
        if (endpoint is Server server)
        {
            server.BeginPacketImmediate(_pPlayerSpellToggled)
                .Write(map.ID)
                .Write(spell);
        }
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
