using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Gear;
using Project1.Core.Networking;
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
    static readonly GearSlotDef[] AttackTargets;

    static CombatSystem()
    {
        DamageToResistanceDefs = Def.Get<DamageResistanceDef>().ToDictionary(r => r.Source, r => r);
        AttackTargets = [.. Def.Get<GearSlotDef>().Where(s => s.AttackTargetWeight > 0)];
    }

    internal static AttackResult ResolveHit(Actor attacker, Actor defender)
    {
        var tick = attacker.World.CurrentTick;
        var attackerId = attacker.RefId;

        var toHit = (int)attacker.Stats.Get(StatDefOf.HitChance);
        var random = attacker.World.Random;
        if (!random.Roll100(toHit))
            return new(tick, attackerId, AttackResultTypes.Miss, DamageInfo.Empty, null, 0);

        var dodge = defender.Stats.Get(StatDefOf.DodgeChance);
        if (random.Roll100((int)dodge))
            return new(tick, attackerId, AttackResultTypes.Dodge, DamageInfo.Empty, null, 0);

        var parry = defender.Stats.Get(StatDefOf.ParryChance);
        if (random.Roll100((int)parry))
            return new(tick, attackerId, AttackResultTypes.Parry, DamageInfo.Empty, null, 0);

        var toCrit = (int)attacker.Stats.Get(StatDefOf.CriticalChance);
        var isCrit = random.Roll100((int)toCrit);
      
        var (damageValue, damageType) = GetAttackDamage(attacker);

        //var bodyPartTargetDef = GearSlotDefOf.Chest;
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

        var preventedDamage = rawValue - finalValue;
        bodyPartGear?.ToolComponent.OnAttackLanded(preventedDamage);
        if (attacker.Gear.GetGear(GearSlotDefOf.Mainhand) is Entity weapon)
            weapon.ToolComponent.OnAttackLanded(finalValue);

        return new(tick, attackerId, isCrit ? AttackResultTypes.Critical : AttackResultTypes.Hit, finalDamage, bodyPartTargetDef, preventedDamage);
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
        world.Events.Post(new WorldCombatEvent(world, runtime.Id, attacker.RefId, result));
        runtime.Record(result);
    }

    internal static void ReplicateOnClient(WorldCombatEvent e)
    {
        var world = (StaticWorld)Client.Instance.World;
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
}
