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
        Registry.MapEventHooksServer.Register<Events_Spells>(HandleSpell);
    }

    private static void HandleSpell(Events_Spells e)
    {
        SendSpell(Server.Instance, e.Entity, e.Spell);
    }
    private static void SendSpell(NetEndpoint endpoint, Entity entity, SpellDef spell)
    {
        endpoint.BeginPacket(_pSpell)
            .Write(entity.RefId)
            .Write(spell);
    }

    private static void OnSpell(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var entity = endpoint.World.Get<Entity>(r.ReadEntityRefId());
        var spell = r.ReadDef<SpellDef>();
        entity.Map.Events.Post(new Events_Spells(entity, spell));
    }
}
