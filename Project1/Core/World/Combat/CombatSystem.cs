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
using Project1.Core.Systems.Stats;
using Project1.Core.Systems.Tools;
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
    }

    internal static AttackResult ResolveHit(Actor attacker, Actor defender)
    {
        var tick = attacker.World.CurrentTick;
        var attackerId = attacker.RefId;

        var toHit = (int)attacker.Stats.Get(StatDefOf.HitChance);
        toHit = 100;
        var random = attacker.World.Random;
        if (!random.Roll100(toHit))
            return new(tick, attackerId, AttackResultTypes.Miss, DamageInfo.Empty, 0, null);

        var dodge = defender.Stats.Get(StatDefOf.DodgeChance);
        if (random.Roll100((int)dodge))
            return new(tick, attackerId, AttackResultTypes.Dodge, DamageInfo.Empty, 0, null);

        var parry = defender.Stats.Get(StatDefOf.ParryChance);
        if (random.Roll100((int)parry))
            return new(tick, attackerId, AttackResultTypes.Parry, DamageInfo.Empty, 0, null);

        var toCrit = (int)attacker.Stats.Get(StatDefOf.CriticalChance);
        var isCrit = random.Roll100((int)toCrit);
      
        var (damageValue, damageType) = GetAttackDamage(attacker);
        damageValue = 30;

        var bodyPartTargetDef = AttackTargets.SelectRandomWeighted(random, s => s.AttackTargetWeight); // TODO make bodypart selection weighted
        var bodyPartGear = defender.Gear.GetGear(bodyPartTargetDef);
        float armor = bodyPartGear?.ToolComponent.Armor ?? 0f;

        var resistandeDef = DamageToResistanceDefs[damageType];
        var resistanceStat = StatSystem.GetDerivedStat(resistandeDef);
        var resistance = defender.Stats.Get(resistanceStat);

        var multiplier = isCrit ? 1.5f : 1f;

        var rawValue = damageValue * multiplier;
        var finalValue = rawValue * 100f / (100f + armor);

        var finalDamage = new DamageInfo((int)finalValue, damageType);

        var preventedDamage = rawValue - finalDamage.Value;

        //bodyPartGear?.ToolComponent.OnAttackLanded(preventedDamage);

        //if (attacker.Gear.GetGear(GearSlotDefOf.Mainhand) is Entity weapon)
        //    weapon.ToolComponent.OnAttackLanded(finalDamage.Value);

        return new(tick, attackerId, isCrit ? AttackResultTypes.Critical : AttackResultTypes.Hit, finalDamage, rawValue, bodyPartTargetDef);
    }

    static public (int value, DamageDef damage) GetAttackDamage(Actor actor)
    {
        var gear = actor.Gear;
        var weapon = gear.Weapon;
        if (weapon is null)
            return ((int)actor.Stats.Get(StatDefOf.Unarmed), DamageDefOf.Blunt);
        var value = (int)weapon.ToolComponent.IntrinsicStats.GetValueOrDefault(StatDefOf.ItemPower).Value;
        var profile = weapon.Profile as GearProfileDef;
        var type = profile.Damage;
        return (value, type);
    }

    static public void PerformAttack(Actor attacker, Actor defender)
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

        var result = ResolveHit(attacker, defender);

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
        session.MarkFlee(actor);

        //if (session.HasAttemptedFlee(actor.RefId))
        //    return;
        var fleeChance = actor.Stats.Get(StatDefOf.FleeChance);
        fleeChance = 100f;// 50f;
        if (!world.Random.Roll100((int)fleeChance))
        {
            // TODO report flee failure
            return;
        }
        actor.Effects.Apply(new EntityEffectWrapper(EffectDefOf.Fled, null, null, 0, duration: EffectDefOf.Fled.BaseDuration));
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
}
