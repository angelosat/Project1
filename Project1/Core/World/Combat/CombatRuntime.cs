using Project1.Core.Components;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Core.Systems.Biology;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Project1.Core.World.Combat;

internal sealed class CombatRuntime(int id, Actor first, Actor second)
{
    internal readonly int Id = id;

    internal readonly Actor First = first, Second = second;

    readonly List<AttackResult> Log = [];

    readonly static int BaseCooldown = Ticks.FromMinutes(10);

    int CooldownFirst = BaseCooldown, CooldownSecond = BaseCooldown;

    internal bool IsResolved { get; private set; }

    internal void Tick()
    {
        if (this.IsResolved)
            throw new InvalidOperationException("Ticked a resolved combat encounter");
        var world = (StaticWorld)this.First.World;
        if (world.Actions.GetAction(this.First) is null)
            this.CooldownFirst -= 1;
        if (CooldownFirst <= 0)
        {
            //PerformAttack(First, Second);
            PerformAttackNew(First, Second);
            CooldownFirst = BaseCooldown;

        }

        if (this.IsResolved)
            return;

        if (world.Actions.GetAction(this.Second) is null)
            this.CooldownSecond -= 1;
        if (CooldownSecond <= 0)
        {
            //PerformAttack(Second, First);
            PerformAttackNew(Second, First);
            CooldownSecond = BaseCooldown;

        }

        //if (this.IsResolved)
        //    throw new InvalidOperationException("Ticked a resolved combat encounter");

        //if (--CooldownFirst <= 0)
        //{
        //    CooldownFirst = BaseCooldown;
        //    //PerformAttack(First, Second);
        //    PerformAttackNew(First, Second);
        //}

        //if (this.IsResolved)
        //    return;

        //if (--CooldownSecond <= 0)
        //{
        //    CooldownSecond = BaseCooldown;
        //    //PerformAttack(Second, First);
        //    PerformAttackNew(Second, First);
        //}
    }
    void PerformAttackNew(Actor attacker, Actor defender)
    {
        var world = (StaticWorld)attacker.World;
        world.Actions.Begin(WorldActionDefOf.Attack, new WorldActionWorker_Attack.Data(attacker.Frontier, attacker) { TargetId = defender.RefId });
    }
    private void PerformAttack(Actor attacker, Actor defender)
    {
        if (!attacker.Net.IsServer)
            return;
        var result = CombatSystem.ResolveHit(attacker, defender);
        switch (result.Type)
        {
            case AttackResultTypes.Miss:
            case AttackResultTypes.Dodge:
            case AttackResultTypes.Parry:
                break;
            case AttackResultTypes.Hit:
                var damage = result.Damage;
                defender.Resources.ApplyDelta(ResourceDefOf.Health, -damage.Value);
                if(defender.Biology.IsIncapacitated)
                    this.IsResolved = true;
                break;
            default:
                throw new UnreachableException();
        }
        Log.Add(result);
    }

    internal Actor Other(Actor actor)
    {
        return actor == this.First ? this.Second : this.First;
    }

    internal void MarkResolved()
    {
        this.IsResolved = true;
    }

    internal void Record(AttackResult result)
    {
        this.Log.Add(result);
    }
}
