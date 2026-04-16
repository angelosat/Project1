using Project1.Core.Effects;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Magic;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Project1.Core.Towns.Services.Spells;

internal static class SpellSystem
{
    static bool initialized;
    static List<EffectScorer> CreateCache()
    {
        if (initialized)
            throw new Exception();
        initialized = true;
        var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(EffectScorer)) && !t.IsAbstract);
        return [.. types.Select(t => ActivatorSafe<EffectScorer>.CreateInstance(t))];
    }
    static public IEnumerable<EffectScorer> Scorers => field ??= CreateCache();
    static Dictionary<EffectDef, EffectScorer> _scorersByEffect => field ??= Scorers.ToDictionary(s => s.Effect);
    static public int Score(Actor actor, EffectDef effect, Def target)
        => _scorersByEffect[effect].Score(actor, target);

    extension(Actor caster)
    {
        public void Cast(SpellDef spell, InteractionTarget target)
        {
            switch (spell.TargetType)
            {
                case TargetType.Null:
                    break;
                case TargetType.Entity:
                    caster.Cast(spell, target.Entity as Actor);
                    break;
                case TargetType.Slot:
                    break;
                case TargetType.BlockEntitySlot:
                    break;
                case TargetType.Cell:
                    break;
                case TargetType.Direction:
                    break;
                case TargetType.BlockEntity:
                    break;
                default:
                    break;
            }
        }
        public void Cast(SpellDef spell, Actor target)
        {
            var effects = spell.Effects;
            var placeholderMagnitude = 50;
            foreach (var fx in effects)
            {
                var fxruntime = new EntityEffectWrapper(fx.effect, fx.target, budget: placeholderMagnitude, ticksPerUnit: 1, duration: spell.EffectDuration);
                target.Effects.Apply(fxruntime);
            }
            caster.Resources.ApplyDelta(ResourceDefOf.Mana, -spell.ManaCost);
        }
    }
}
