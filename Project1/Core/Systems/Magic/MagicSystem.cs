using Project1.Core.Entities.Actors;
using Project1.Core.Events;
using Project1.Core.Helpers;
using Project1.Core.Resources;
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

    public static bool CanCastSpell(Actor actor, SpellDef spell)
        => actor.Resources.GetValue(ResourceDefOf.Mana) >= spell.ManaCost;

    public static SpellDef? ConsiderDefensiveSpell(Actor actor, EffectDef effect, Def effectTarget)
        => actor.Spellbook.Spells
        .Where(s => CanCastSpell(actor, s))
        .FirstOrDefault(s => s.Effects.Keys.Any(e => e.Def == effect && e.Target == effectTarget));

    public static SpellDef? ConsiderOffensiveSpell(Actor actor, Actor target)
       => actor.Spellbook.Spells
       .Where(s => CanCastSpell(actor, s))
       .FirstOrDefault(s => s.Effects.Keys.Any(e => e.Def == EffectDefOf.SpellDamage && e.Target == ResourceDefOf.Health));
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

[NetworkReplicate]
public record struct ActorCastSpellEvent(StaticWorld World, EntityRefId ActorId, EntityRefId TargetId, SpellDef SpellDef)
    : IWorldEvent, ISerializableNewNew<ActorCastSpellEvent>
{
    public static ActorCastSpellEvent Create(IDataReader r)
        => new()
        {
            ActorId = r.ReadEntityRefId(),
            TargetId = r.ReadEntityRefId(),
            SpellDef = r.ReadDef<SpellDef>()
        };

    public readonly IDataWriter Write(IDataWriter w)
        => w
        .Write(this.ActorId)
        .Write(this.TargetId)
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