using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Core.Systems.Effects;
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
            //spell.Instant.CreateData().Perform(target);
            //var placeholderMagnitude = 50;
            //var effects = spell.Periodic;
            //foreach (var config in effects)
            //{
            //    var data = config.CreateData();
            //    var runtime = new EntityEffectRuntime()
            //    {
            //        Dto = data,
            //        TickInterval = spell.TickPeriod,
            //        TicksRemaining = spell.TickCount
            //    };
            //    target.Effects.Apply(runtime);
            //}
            //caster.Resources.ApplyDelta(ResourceDefOf.Mana, -spell.ManaCost);
            foreach (var (config, schedule) in spell.EffectsNew)
            {
                target.World.Events.Post(new ActorEffectAppliedNewNewEvent((StaticWorld)target.World,
                                                                           target.RefId,
                                                                           config,
                                                                           schedule));
            }
            caster.Resources.ApplyDelta(ResourceDefOf.Mana, -spell.ManaCost);
        }
    }

    public static void SpellSuccess(Actor caster, Actor target, SpellDef spell)
    {
        var world = (StaticWorld)target.World;
        var cost = spell.ManaCost;
        caster.Resources.ApplyDelta(ResourceDefOf.Mana, -cost);
        //foreach (var config in spell.Periodic)
        //{
        //    world.Events.Post(new ActorEffectAppliedNewNewEvent(world,
        //                                                        target.RefId,
        //                                                        config,
        //                                                        EffectSchedule.Timed(spell.TickPeriod, spell.TickCount),
        //                                                        new() { School = spell.School }));
        //}
        foreach (var (config, schedule) in spell.EffectsNew)
        {
            world.Events.Post(new ActorEffectAppliedNewNewEvent(world,
                                                                target.RefId,
                                                                config,
                                                                schedule,//EffectSchedule.Timed(spell.TickPeriod, spell.TickCount),
                                                                new() { School = spell.School }));
        }
    }
}
