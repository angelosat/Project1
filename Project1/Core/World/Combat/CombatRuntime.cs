using Project1.Core.Attributes;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.Systems.Biology;
using Project1.Core.World.Actions;
using System;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.World.Combat;

internal sealed class CombatRuntime(int id, Actor first, Actor second)
{
    internal readonly int Id = id;

    internal readonly Actor First = first, Second = second;

    StaticWorld World => field ??= (StaticWorld)this.First.World;

    readonly List<AttackResult> Log = [];

    readonly static int BaseCooldown = Ticks.FromMinutes(1);

    bool Started;

    public bool IsUnstarted => !this.Started;

    int CooldownFirst,
        CooldownSecond;

    internal bool IsResolved { get; private set; }

    internal Actor? Winner
    {
        get
        {
            if (!this.IsResolved)
                return null;
            if (this.First.Biology.IsIncapacitated)
                return this.Second;
            else if (this.Second.Biology.IsIncapacitated)
                return this.First;
            return null;
            //throw new UnreachableException();
        }
    }

    internal Actor? Loser
    {
        get
        {
            if (!this.IsResolved)
                return null;
            if (this.First.Biology.IsIncapacitated)
                return this.First;
            else if (this.Second.Biology.IsIncapacitated)
                return this.Second;
            return null;
            //throw new UnreachableException();
        }
    }

    internal void Tick()
    {
        if (this.IsResolved)
            throw new InvalidOperationException("Ticked a resolved combat encounter");

        var world = (StaticWorld)this.First.World;
        if(this.CooldownFirst > 0)
            this.CooldownFirst -= 1;
        else if (world.Actions.GetAction(this.First) is null)
        {
            this.PerformAttackNew(this.First, this.Second);
            this.CooldownFirst = ResetCooldown(this.First);
        }

        if (this.IsResolved)
            return;

        if (this.IsUnstarted)
            return;

        if (this.CooldownSecond > 0)
            this.CooldownSecond -= 1;
        else if (world.Actions.GetAction(this.Second) is null)
        {
            this.PerformAttackNew(this.Second, this.First);
            this.CooldownSecond = ResetCooldown(this.Second);
        }
    }

    int ResetCooldown(Actor actor)
    {
        var dexterity = actor.Attributes.Get(AttributeDefOf.Dexterity);
        return (int)(BaseCooldown * (1 - dexterity / 100f));
    }

    void PerformAttackNew(Actor attacker, Actor defender)
    {
        var world = (StaticWorld)attacker.World;
        world.Actions.Begin(WorldActionDefOf.Attack, new WorldActionWorker_Attack.Data(attacker) { TargetId = defender.RefId });
    }

    internal Actor GetOther(Actor actor)
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
        if(!this.Started)
        {
            this.World.Actions.Interrupt(this.Second);
            this.Started = true;
        }
    }
}
