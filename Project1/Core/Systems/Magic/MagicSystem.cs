using Project1.Core.Entities.Actors;
using Project1.Core.Events;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Core.Systems.Effects;
using Project1.Framework;
using Project1.Framework.Serialization;
using System.Linq;

#nullable enable

namespace Project1.Core.Systems.Magic;

[EnsureStaticCtorCall]
internal static class MagicSystem
{
    static MagicSystem()
    {
        Registry.WorldEventHooksClient.Register<ActorLearnSpellEvent>(OnActorLearnSpell);
        Registry.WorldEventHooksServer.Register<ActorLearnSpellEvent>(OnActorLearnSpell);
    }

    private static void OnActorLearnSpell(ActorLearnSpellEvent e)
    {
        var actor = e.World.Resolve<Actor>(e.ActorId);
        var spell = e.SpellDef;
        actor.Spellbook.Add(spell);
    }

    public static void TeachSpell(Actor actor, SpellDef spell)
    {
        actor.World.Events.Post(new ActorLearnSpellEvent((StaticWorld)actor.World, actor.RefId, spell));
    }

    public static bool CanCastSpell(Actor actor)
        => true;

    public static SpellDef? ConsiderSpell(Actor actor, EffectDef effect, Def target)
        => actor.Spellbook.Spells.FirstOrDefault(s => s.Effects.Keys.Any(e => e.Def == effect && e.Target == target));
}

[NetworkReplicate]
public record struct ActorLearnSpellEvent(StaticWorld World, EntityRefId ActorId, SpellDef SpellDef)
    : IWorldEvent, ISerializableNewNew<ActorLearnSpellEvent>
{
    public static ActorLearnSpellEvent Create(IDataReader r)
        => new()
        {
            ActorId = r.ReadEntityRefId(),
            SpellDef = r.ReadDef<SpellDef>()
        };

    public readonly IDataWriter Write(IDataWriter w)
        => w
        .Write(this.ActorId)
        .Write(this.SpellDef);
}

//[NetworkReplicate]
//public record struct ActorFinishSpellEvent(StaticWorld World, EntityRefId ActorId, SpellDef SpellDef)
//    : IWorldEvent, ISerializableNewNew<ActorFinishSpellEvent>
//{
//    public static ActorFinishSpellEvent Create(IDataReader r)
//        => new()
//        {
//            ActorId = r.ReadEntityRefId(),
//            SpellDef = r.ReadDef<SpellDef>()
//        };

//    public readonly IDataWriter Write(IDataWriter w)
//        => w
//        .Write(this.ActorId)
//        .Write(this.SpellDef);
//}