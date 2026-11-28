using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Gear;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Core.Skills;
using Project1.Core.Stats;
using Project1.Core.Systems.Biology;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Magic;
using Project1.Core.Systems.Stats;
using Project1.Core.Systems.Tools;
using Project1.Core.Towns.Services.Spells;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable enable

namespace Project1.Core.World.Combat;

[EnsureStaticCtorCall]
internal static class CombatSystem
{
    static readonly Dictionary<DamageDef, DamageResistanceDef> DamageToResistanceDefs;
    static readonly GearSlotDef[] AttackTargets;

    static CombatSystem()
    {
        DamageToResistanceDefs = Def.Get<DamageResistanceDef>().ToDictionary(r => r.Source, r => r);
        AttackTargets = [.. Def.Get<GearSlotDef>().Where(s => s.AttackTargetWeight > 0)];

        Registry.WorldEventHooksClient.Register<WorldCombatAttackEvent>(IncrementTrackingStats);
        Registry.WorldEventHooksServer.Register<WorldCombatAttackEvent>(IncrementTrackingStats);

        Registry.WorldEventHooksClient.Register<WorldCombatFleeEvent>(OnWorldCombatFlee);
        Registry.WorldEventHooksServer.Register<WorldCombatFleeEvent>(OnWorldCombatFlee);

        Registry.WorldEventHooksClient.Register<ActorCastSpellEvent>(OnActorCastSpell);
        Registry.WorldEventHooksServer.Register<ActorCastSpellEvent>(OnActorCastSpell);
    }

    internal static AttackResult ResolveMeleeHit(Actor attacker, Actor defender)
    {
        var tick = attacker.World.CurrentTick;
        var attackerId = attacker.RefId;

        var toHit = (int)attacker.Stats.GetFinal(StatDefOf.MeleeHitChance);
        toHit = 100;
        var random = attacker.World.Random;
        if (!random.Roll100(toHit))
            return new(tick, attackerId, AttackResultTypes.Miss, DamageInfo.Empty, 0, null);

        var dodge = defender.Stats.GetFinal(StatDefOf.DodgeChance);
        if (random.Roll100((int)dodge))
            return new(tick, attackerId, AttackResultTypes.Dodge, DamageInfo.Empty, 0, null);

        var parry = defender.Stats.GetFinal(StatDefOf.ParryChance);
        if (random.Roll100((int)parry))
            return new(tick, attackerId, AttackResultTypes.Parry, DamageInfo.Empty, 0, null);

        var toCrit = (int)attacker.Stats.GetFinal(StatDefOf.CriticalChance);
        var isCrit = random.Roll100((int)toCrit);
      
        var (damageValue, damageType) = GetAttackDamage(attacker);
        damageValue = 30;

        var bodyPartTargetDef = AttackTargets.SelectRandomWeighted(random, s => s.AttackTargetWeight); // TODO make bodypart selection weighted
        var bodyPartGear = defender.Gear.GetGear(bodyPartTargetDef);
        float armor = bodyPartGear?.ToolComponent.Armor ?? 0f;

        var resistandeDef = DamageToResistanceDefs[damageType];
        var resistanceStat = StatSystem.GetDerivedStat(resistandeDef);
        var resistance = defender.Stats.GetFinal(resistanceStat);

        var multiplier = isCrit ? 1.5f : 1f;

        var rawValue = damageValue * multiplier;
        var finalValue = rawValue * 100f / (100f + armor);

        var finalDamage = new DamageInfo((int)finalValue, damageType);

        var preventedDamage = rawValue - finalDamage.Value;

        return new(tick, attackerId, isCrit ? AttackResultTypes.Critical : AttackResultTypes.Hit, finalDamage, rawValue, bodyPartTargetDef);
    }

    static public (int value, DamageDef damage) GetAttackDamage(Actor actor)
    {
        var gear = actor.Gear;
        var weapon = gear.Weapon;
        if (weapon is null)
            return ((int)actor.Stats.GetFinal(StatDefOf.Unarmed), DamageDefOf.Blunt);
        var value = (int)weapon.ToolComponent.IntrinsicStats.GetValueOrDefault(StatDefOf.ItemPower).Value;
        var profile = weapon.Profile as GearProfileDef;
        var type = profile.Damage;
        return (value, type);
    }

    static public void ResolveWeaponAttack(Actor attacker, Actor defender)
    {
        if (!attacker.Net.IsServer)
            return;

        var world = (StaticWorld)attacker.World;

        if (!world.Combat.TryGet(attacker, out var runtime))
            world.Combat.Engage(attacker, defender);

        if (runtime.GetOther(attacker) != defender)
            throw new InvalidOperationException(
                $"Actor mismatch for existing combat runtime."
                );

        var result = ResolveMeleeHit(attacker, defender);

        switch (result.Type)
        {
            case AttackResultTypes.Miss:
                if (GetWeaponSkill(attacker) is SkillDef skill)
                    defender.Skills.ApplyXp(skill, 1);
                break;
            case AttackResultTypes.Dodge:
            case AttackResultTypes.Parry:
                break;
            case AttackResultTypes.Critical:
            case AttackResultTypes.Hit:
                var damage = result.Damage;
                defender.Resources.ApplyDelta(ResourceDefOf.Health, -damage.Value);
                if (defender.Biology.IsIncapacitated)
                    runtime.MarkResolved();
                break;
            default:
                throw new UnreachableException();
        }
        $"{attacker} {result.Type} {defender} {result.Damage}".ToConsole();
        world.Events.Post(new WorldCombatAttackEvent(world, runtime.Id, attacker.RefId, result));
        runtime.Record(result);
    }

    static public SkillDef GetWeaponSkill(Actor actor)
        => actor.Gear.Weapon?.GetProfile<GearProfileDef>()?.WeaponSkill ?? SkillDefOf.Unarmed;

    internal static void IncrementTrackingStats(WorldCombatAttackEvent e)
    {
        if (e.Result.Damage.Value == 0)
            return;
        var world = e.World;
        var attacker = world.Resolve<Actor>(e.AttackerId);
        if (attacker.Gear.GetGear(GearSlotDefOf.Mainhand) is Entity weapon)
            weapon.ToolComponent.OnAttackLanded(e.Result.Damage.Value);
        var combat = world.Combat.Get(e.AttackerId);
        var defender = combat.GetOther(attacker);
        var slotDef = e.Result.BodyPart;
        if (slotDef is null)
            return;
        if (defender.Gear.GetGear(slotDef) is Entity gear)
            gear.ToolComponent.OnAttackLanded(e.Result.DamagePrevented);
    }

    internal static void AttemptFlee(Actor actor)
    {
        var world = (StaticWorld)actor.World;
        var session = world.Combat.Get(actor);
        ArgumentNullException.ThrowIfNull(session);
        session.MarkFlee(actor);
        var fleeChance = actor.Stats.GetFinal(StatDefOf.FleeChance);
        fleeChance = 100f;// 50f;
        if (!world.Random.Roll100((int)fleeChance))
        {
            // TODO report flee failure
            return;
        }
        //actor.Effects.Apply(new EntityEffectRuntime()
        //{
        //    Dto = new EffectConfig_Flee().CreateData()
        //});
        world.Events.Post(new ActorEffectAppliedNewNewEvent(world, actor.RefId,
            new EffectConfig_Flee(),
            EffectSchedule.Timed(Ticks.FromMinutes(30), 1)));
        world.Events.Post(new WorldCombatFleeEvent(world, actor.RefId));
        // TODO report flee success
    }

    private static void OnWorldCombatFlee(WorldCombatFleeEvent e)
    {
        var world = e.World;
        var actor = world.Resolve<Actor>(e.ActorId);
        var session = world.Combat.Get(e.ActorId);
        var other = session.GetOther(actor);
        world.Actions.Interrupt(other);
        session.MarkResolved();
    }

    internal static void SpellSuccess(Actor caster, Actor target, SpellDef spell)
    {
        var world = (StaticWorld)caster.World;
        var cost = spell.ManaCost;
        caster.Resources.ApplyDelta(ResourceDefOf.Mana, -cost);
        // TODO apply instant effect
        //foreach (var (key, mag) in spell.Effects)
        //    world.Events.Post(new ActorEffectAppliedNewEvent(world, target.RefId, key.Def, key.Target, mag));
        SpellSystem.SpellSuccess(caster, target, spell);
        //foreach (var config in spell.Periodic)
        //{
        //    var data = config.CreateData();
        //    world.Events.Post(new ActorEffectAppliedNewNewEvent(world, target.RefId, data, spell.TickPeriod, spell.TickCount));
        //}
    }

    private static void OnActorCastSpell(ActorCastSpellEvent e)
    {
        var world = e.World;
        if (!world.Net.IsServer)
            return;
        var actor = world.Resolve<Actor>(e.ActorId);
        if (!world.Combat.TryGet(actor, out var session))
            return;
        var spell = e.SpellDef;
        switch(spell.Category)
        {
            case SpellCategory.Neutral:
            case SpellCategory.Defensive:
                SpellSuccess(actor, actor, spell);
                break;

            case SpellCategory.Offensive:
                var target = world.Resolve<Actor>(e.TargetId);
                ResolveMagicHit(actor, target, spell);
                break;

            default:
                throw new UnreachableException();
        }
    }

    internal static void ResolveMagicHit(Actor caster, Actor target, SpellDef spell)
    {
        var tick = caster.World.CurrentTick;
        var attackerId = caster.RefId;

        var toHit = (int)caster.Stats.GetFinal(StatDefOf.SpellHitChance);
        toHit = 100;
        var world = (StaticWorld)caster.World;
        var random = world.Random;
        if (!random.Roll100(toHit))
            return;// new(tick, attackerId, AttackResultTypes.Miss, DamageInfo.Empty, 0, null);

        // TODO create spell crit chance stat
        //var toCrit = (int)caster.Stats.GetFinal(StatDefOf.CriticalChance);
        //var isCrit = random.Roll100((int)toCrit);

        var damageType = spell.School.DamageType;

        var resistandeDef = DamageToResistanceDefs[damageType];
        var resistanceStat = StatSystem.GetDerivedStat(resistandeDef);
        var resistance = target.Stats.GetFinal(resistanceStat);

        var multiplier = 1f;// isCrit ? 1.5f : 1f;

        var finalMagnitudeMultiplier = multiplier * (1f - resistance);

        // TODO hit chance should only be the deciding factor on whether the spell casts at all
        // TODO and the final magnitude should be calculated by the spell effect itself on application
        SpellSystem.SpellSuccess(caster, target, spell); 
        
        //foreach(var (effectKey, mag) in spell.Effects)
        //{
        //    var final = mag * finalMagnitudeMultiplier;
        //    world.Events.Post(new ActorEffectAppliedNewEvent(world, target.RefId, effectKey.Def, effectKey.Target, (int)final));
        //}
        //return new(tick, attackerId, isCrit ? AttackResultTypes.Critical : AttackResultTypes.Hit, finalDamage, rawValue, bodyPartTargetDef);
    }
}
