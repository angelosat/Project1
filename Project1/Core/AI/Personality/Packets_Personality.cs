using Project1.Core.AI.Personality;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Framework;

namespace Project1.Core.AI.Presonality;

[EnsureStaticCtorCall]
internal static class Packets_Personality
{
    static readonly PacketId
        _pPlayerChangeTraitVal = Registry.PacketHandlers.Register(ReceivePlayerChangeTraitVal),
        _pTraitValChanged = Registry.PacketHandlers.Register(ReceiveTraitValChanged)
        ;

    static Packets_Personality()
    {
        Registry.PlayerInputEventHooks.Register<PlayerChangeTraitValueEvent>(HandlePlayerChangeTraitValue);
        Registry.WorldEventHooksServer.Register<TraitValueChangedEvent>(HandleTraitValueChanged);
    }

    private static void HandleTraitValueChanged(TraitValueChangedEvent e)
    {
        SendTraitValChanged(Server.Instance, e.Actor.RefId, e.Trait, e.Value);
    }

    private static void HandlePlayerChangeTraitValue(PlayerChangeTraitValueEvent e)
    {
        if(Ingame.Net.IsServer)
        {
            e.Actor.Personality.SetValue(e.Trait, e.Value);
        }
        SendPlayerChangeTraitVal(Client.Instance, e.Actor.RefId, e.Trait, e.Value);
    }

    private static void SendPlayerChangeTraitVal(Client client, EntityRefId actor, TraitDef trait, float value)
    {
        client.BeginPacketImmediate(_pPlayerChangeTraitVal)
            .Write(actor)
            .Write(trait)
            .Write(value);
    }

    private static void ReceivePlayerChangeTraitVal(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var actorid = r.ReadEntityRefId();
        var trait = r.ReadDef<TraitDef>();
        var val = r.ReadSingle();
        var actor = endpoint.World.Get(actorid);
        var comp = actor.GetComponent<PersonalityComponent>();
        comp.SetValue(trait, val);

    }

    private static void SendTraitValChanged(Server server, EntityRefId actor, TraitDef trait, float value)
    {
        server.BeginPacketImmediate(_pTraitValChanged)
            .Write(actor)
            .Write(trait)
            .Write(value);
    }

    private static void ReceiveTraitValChanged(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var actor = endpoint.World.Get<Actor>(r.ReadEntityRefId());
        var trait = r.ReadDef<TraitDef>();
        var val = r.ReadSingle();
        actor.Personality.SetValue(trait, val);
    }
}
