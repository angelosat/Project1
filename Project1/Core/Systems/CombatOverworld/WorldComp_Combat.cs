using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Core.Stats;
using Project1.Core.Systems.Stats;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Systems.CombatOverworld;

internal sealed class CombatHitResult(DamageInfo finalDamage)
{
    DamageInfo FinalDamage = finalDamage;
}

public enum CombatResults { Hit, Miss, Dodge, Parry }

[EnsureStaticCtorCall]
internal static class CombatSystem
{
    static readonly Random Random = new();
    static readonly Dictionary<DamageDef, DamageResistanceDef> DamageToResistanceDefs;

    static CombatSystem()
    {
        DamageToResistanceDefs = Def.Get<DamageResistanceDef>().ToDictionary(r => r.Source, r => r);
    }

    internal static CombatResults ResolveHit(Actor attacker, Actor defender, out DamageInfo? finalDamage)
    {
        var toHit = (int)attacker.Stats.Get(StatDefOf.HitChance);
        if (!Random.Roll100(toHit))
        {
            finalDamage = null;
            return CombatResults.Miss;
        }
        var dodge = defender.Stats.Get(StatDefOf.DodgeChance);
        if (Random.Roll100((int)dodge))
        {
            finalDamage = null;
            return CombatResults.Dodge;
        }
        var parry = defender.Stats.Get(StatDefOf.ParryChance);
        if (Random.Roll100((int)parry))
        {
            finalDamage = null;
            return CombatResults.Parry;
        }

        var damageValue = attacker.Gear.GetDamage();
        var damageType = attacker.Gear.GetDamageType();
       
        var armor = defender.Stats.Get(StatDefOf.Armor);
        var resistandeDef = DamageToResistanceDefs[damageType];
        var resistanceStat = StatSystem.GetDerivedStat(resistandeDef);
        var resistance = defender.Stats.Get(StatSystem.GetDerivedStat(resistanceStat));

        finalDamage = new DamageInfo((int)damageValue, damageType);
        // TODO calculate damage mitigation

        return CombatResults.Hit;
    }
}

internal sealed class WorldComp_Combat(WorldBase world) : WorldComp(world)
{
    int NextId => ++field;

    readonly Dictionary<int, CombatRuntime> ActiveCombatSessions = [];

    readonly Dictionary<EntityRefId, int> ActorsToCombat = [];

    public void Engage(Actor first, Actor second)
    {
        var id = this.NextId;
        var runtime = new CombatRuntime(id, first, second);
        this.RegisterCombat(first, second, id, runtime);
    }

    public bool IsEngaged(Actor actor)
        => this.ActorsToCombat.ContainsKey(actor.RefId);

    internal override void Tick()
    {
        //var unResolved = this.ActiveCombatSessions.Values.ToArray();
        //foreach (var combat in unResolved)
        //    combat.Tick();
        //foreach (var combat in unResolved)
        //    if (combat.IsResolved)
        //        this.UnregisterCombat(combat);

        var toRemove = new List<CombatRuntime>(this.ActiveCombatSessions.Count);
        foreach (var combat in this.ActiveCombatSessions.Values)
        {
            combat.Tick();
            if (combat.IsResolved)
                toRemove.Add(combat);
        }
        foreach (var combat in toRemove)
            this.UnregisterCombat(combat);
    }

    private void RegisterCombat(Actor first, Actor second, int id, CombatRuntime runtime)
    {
        this.ActiveCombatSessions.Add(id, runtime);
        this.ActorsToCombat.Add(first.RefId, id);
        this.ActorsToCombat.Add(second.RefId, id);
    }

    private void UnregisterCombat(CombatRuntime combat)
    {
        this.ActiveCombatSessions.Remove(combat.Id);
        this.ActorsToCombat.Remove(combat.First.RefId);
        this.ActorsToCombat.Remove(combat.Second.RefId);
    }
}

public enum CombatStates { InProgress, Resolved }

internal sealed class CombatRuntime(int id, Actor first, Actor second)
{
    internal readonly int Id = id;

    internal readonly Actor First = first, Second = second;

    int CooldownFirst, CooldownSecond;
    readonly int BaseCooldown = Ticks.FromMinutes(10);

    internal bool IsResolved { get; private set; }

    internal void Tick() 
    {
        if (this.CooldownFirst-- <= 0)
        {
            this.CooldownFirst = this.BaseCooldown;
            PerformAttack(this.First, this.Second);
        }

        if (this.CooldownSecond-- <= 0)
        {
            this.CooldownSecond = this.BaseCooldown;
            PerformAttack(this.Second, this.First);
        }

        // TODO finish combat when somone dies or retreats
    }

    private void PerformAttack(Actor attacker, Actor defender)
    {
        switch (CombatSystem.ResolveHit(attacker, defender, out var damage))
        {
            case CombatResults.Miss:
            case CombatResults.Dodge:
            case CombatResults.Parry:
                break;
            case CombatResults.Hit:
                defender.Resources.ApplyDelta(ResourceDefOf.Health, -damage.Value.Value);
                break;
            default:
                throw new UnreachableException();
        }
    }
}

record struct DamageInfo(int Value, DamageDef Type)
{
    //internal readonly DamageTypeDef Type = type;
    //internal readonly int DamageValue = value;
}
