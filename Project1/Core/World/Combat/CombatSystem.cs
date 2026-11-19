using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Core.Stats;
using Project1.Core.Systems.Biology;
using Project1.Core.Systems.Stats;
using Project1.Core.Systems.Tools;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.World.Combat;

[EnsureStaticCtorCall]
internal static class CombatSystem
{
    static readonly Dictionary<DamageDef, DamageResistanceDef> DamageToResistanceDefs;

    static CombatSystem()
    {
        DamageToResistanceDefs = Def.Get<DamageResistanceDef>().ToDictionary(r => r.Source, r => r);
    }

    internal static AttackResult ResolveHit(Actor attacker, Actor defender)
    {
        var tick = attacker.World.CurrentTick;
        var attackerId = attacker.RefId;

        var toHit = (int)attacker.Stats.Get(StatDefOf.HitChance);
        var random = attacker.World.Random;
        if (!random.Roll100(toHit))
            return new(tick, attackerId, AttackResultTypes.Miss, DamageInfo.Empty);

        var dodge = defender.Stats.Get(StatDefOf.DodgeChance);
        if (random.Roll100((int)dodge))
            return new(tick, attackerId, AttackResultTypes.Dodge, DamageInfo.Empty);

        var parry = defender.Stats.Get(StatDefOf.ParryChance);
        if (random.Roll100((int)parry))
            return new(tick, attackerId, AttackResultTypes.Parry, DamageInfo.Empty);

        var toCrit = (int)attacker.Stats.Get(StatDefOf.CriticalChance);
        var isCrit = random.Roll100((int)toCrit);
      
        var (damageValue, damageType) = GetAttackDamage(attacker);

        var bodyPartTargetDef = defender.Gear.SlotDefs.SelectRandom(random); // TODO make bodypart selection weighted
        var bodyPartGear = defender.Gear.GetGear(bodyPartTargetDef);
        float armor = bodyPartGear?.ToolComponent.Armor ?? 0f;

        var resistandeDef = DamageToResistanceDefs[damageType];
        var resistanceStat = StatSystem.GetDerivedStat(resistandeDef);
        var resistance = defender.Stats.Get(resistanceStat);

        var multiplier = isCrit ? 1.5f : 1f;

        var rawValue = damageValue * multiplier;
        var finalValue = rawValue * 100f / (100f + armor);

        var finalDamage = new DamageInfo((int)finalValue, damageType);

        return new(tick, attackerId, isCrit ? AttackResultTypes.Critical : AttackResultTypes.Hit, finalDamage);
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

        if(!world.Combat.TryGet(attacker, out var runtime))
            runtime = world.Combat.Engage(attacker, defender);

        if (runtime.GetOther(attacker) != defender)
            throw new InvalidOperationException(
                $"Actor mismatch for existing combat runtime."
                );

        var result = ResolveHit(attacker, defender);

        switch (result.Type)
        {
            case AttackResultTypes.Miss:
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
        world.Events.Post(new WorldCombatEvent(runtime, attacker, result));
        runtime.Record(result);
    }
}
