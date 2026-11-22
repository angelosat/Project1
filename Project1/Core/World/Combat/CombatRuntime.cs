using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.Systems.Biology;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#nullable enable

namespace Project1.Core.World.Combat;

internal sealed class CombatRuntime(int id, Actor first, Actor second)
{
    internal readonly int Id = id;

    const int PendingCooldown = -1;

    internal readonly Actor First = first, Second = second;

    StaticWorld World => field ??= (StaticWorld)this.First.World;

    readonly List<AttackResult> Log = [];

    readonly HashSet<EntityRefId> FleeAttempts = [];

    readonly static int BaseCooldown = Ticks.FromMinutes(1);

    bool Started;

    public bool IsUnstarted => !this.Started;

    int CooldownFirst,
        CooldownSecond;

    //int[] Cooldowns;

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
        }
    }

    internal void Tick()
    {
        if (this.IsResolved)
            throw new InvalidOperationException("Ticked a resolved combat encounter");

        var world = (StaticWorld)this.First.World;
        if (this.CooldownFirst > 0)
            this.CooldownFirst -= 1;
        else if (this.CooldownFirst == 0 &&
                world.Actions.GetAction(this.First) is null)
            DecideAction(this.First, this.Second);
        else if (this.CooldownFirst == PendingCooldown)
            this.TriggerCooldown(this.First);

        if (this.IsResolved)
            return;

        if (this.IsUnstarted)
            return;

        if (this.CooldownSecond > 0)
            this.CooldownSecond -= 1;
        else if (this.CooldownSecond == 0 &&
            world.Actions.GetAction(this.Second) is null)
            DecideAction(this.Second, this.First);
        else if (this.CooldownSecond == PendingCooldown)
            this.TriggerCooldown(this.Second);
    }

    static int ResetCooldown(Actor actor)
    {
        return BaseCooldown;

        //var speed = actor.Stats.Get(StatDefOf.SwingSpeed);
        //speed += 100;
        //speed /= 100;
        //return (int)(BaseCooldown * speed);

        //var dexterity = actor.Attributes.Get(AttributeDefOf.Dexterity);
        //return (int)(BaseCooldown * (1 - dexterity / 100f));
    }

    static void DecideAction(Actor attacker, Actor defender)
    {
        //world.Actions.Begin(WorldActionDefOf.Attack, new WorldActionWorker_Attack.Data(attacker) { TargetId = defender.RefId });
        var (actionDef, actionData) = ActorCombatAISystem.Decide(attacker, defender);
        if (actionDef is null)
            return;
        var world = (StaticWorld)attacker.World;
        world.Actions.Begin(actionDef, actionData);
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
        this.PendCooldown(result.ActorId);
    }

    Actor GetParticipant(EntityRefId actorId)
    {
        if (actorId == this.First.RefId)
            return this.First;
        if (actorId == this.Second.RefId)
            return this.Second;

        throw new ArgumentException(
            $"Invalid {nameof(actorId)} for combat session {this.Id}");
    }

    private void TriggerCooldown(Actor actor)
    {
        if (actor == this.First)
            this.CooldownFirst = ResetCooldown(actor);
        else if (actor == this.Second)
            this.CooldownSecond = ResetCooldown(actor);
        else
            throw new ArgumentException(
                $"Invalid {nameof(actor)} for combat session {this.Id}");
    }

    void PendCooldown(EntityRefId actorId)
    {
        var actor = this.GetParticipant(actorId);
        if (actor == this.First)
            this.CooldownFirst = PendingCooldown;
        else if (actor == this.Second)
            this.CooldownSecond = PendingCooldown;
        else
            throw new ArgumentException(
                $"Invalid {nameof(actor)} for combat session {this.Id}");
    }

    internal bool CanAct(Actor actor)
    {
        if (actor == this.First)
            return this.CooldownFirst == 0;
        else if (actor == this.Second)
            return this.CooldownSecond == 0;

        throw new UnreachableException();
    }

    internal bool HasAttemptedFlee(EntityRefId refId)
        => this.FleeAttempts.Contains(refId);

    internal void MarkFlee(Actor actor)
        => this.FleeAttempts.Add(actor.RefId);

    internal bool CanFlee(Actor actor)
        => !this.HasAttemptedFlee(actor.RefId);
}
