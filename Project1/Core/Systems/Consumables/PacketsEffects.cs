using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Systems.Magic;
using Project1.Framework;

namespace Project1.Core.Systems.Consumables;

[EnsureStaticCtorCall]
internal static class PacketsEffects
{
    internal static readonly PacketId _pSpell = Registry.PacketHandlers.Register(OnSpell);

    static PacketsEffects()
    {
        Registry.MapEventHooksServer.Register<SpellCastEvent>(HandleSpell);
    }

    private static void HandleSpell(SpellCastEvent e)
    {
        SendSpell(Server.Instance, e.Caster, e.Target, e.Spell);
    }
    private static void SendSpell(NetEndpoint endpoint, Entity entity, InteractionTarget target, SpellDef spell)
    {
        endpoint.BeginPacket(_pSpell)
            .Write(entity.RefId)
            .Write(target)
            .Write(spell);
    }

    private static void OnSpell(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var entity = endpoint.World.Get<Entity>(r.ReadEntityRefId());
        var target = r.ReadTarget(endpoint.World);
        var spell = r.ReadDef<SpellDef>();
        entity.Map.Events.Post(new SpellCastEvent(entity, target, spell));
    }
}
