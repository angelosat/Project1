using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Stats;
using Project1.Core.Systems.Stats;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.World.Combat;

[EnsureStaticCtorCall]
internal static class CombatSystem
{
    static readonly Random Random = new();
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
        if (!Random.Roll100(toHit))
            return new(tick, attackerId, AttackResultTypes.Miss, DamageInfo.Empty);

        var dodge = defender.Stats.Get(StatDefOf.DodgeChance);
        if (Random.Roll100((int)dodge))
            return new(tick, attackerId, AttackResultTypes.Dodge, DamageInfo.Empty);

        var parry = defender.Stats.Get(StatDefOf.ParryChance);
        if (Random.Roll100((int)parry))
            return new(tick, attackerId, AttackResultTypes.Parry, DamageInfo.Empty);

        var damageValue = attacker.Gear.GetDamage();
        var damageType = attacker.Gear.GetDamageType();

        var armor = defender.Stats.Get(StatDefOf.Armor);
        var resistandeDef = DamageToResistanceDefs[damageType];
        var resistanceStat = StatSystem.GetDerivedStat(resistandeDef);
        var resistance = defender.Stats.Get(resistanceStat);

        var finalDamage = new DamageInfo((int)damageValue, damageType);
        // TODO calculate damage mitigation

        return new(tick, attackerId, AttackResultTypes.Hit, finalDamage);
    }
}
