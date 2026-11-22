using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.Stats;
using Project1.Core.World.Frontiers;
using Project1.Framework.Helpers;
using SharpDX;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.World.Combat;

internal sealed class WorldComp_Combat : WorldComp
{
    int NextId => ++field;

    readonly Dictionary<int, CombatRuntime> ActiveCombatSessions = [];

    readonly Dictionary<EntityRefId, int> ActorsToCombat = [];

    public WorldComp_Combat(WorldBase world) : base(world)
    {
        world.Events.ListenTo<CombatBeginEvent>(OnCombatBegin);
        world.Events.ListenTo<CombatEndEvent>(OnCombatEnd);
    }

    private void OnCombatBegin(CombatBeginEvent e)
    {
        this.RegisterCombat(/*e.CombatSessionId,*/
                            this.World.Resolve<Actor>(e.Attacker),
                            this.World.Resolve<Actor>(e.Defender));
    }

    private void OnCombatEnd(CombatEndEvent e)
    {
        this.UnregisterCombat(e.CombatSessionId);
    }

    public void Engage(Actor instigator, Actor target)
    {
        this.World.Events.Post(new CombatBeginEvent(this.World, instigator.RefId, target.RefId));
    }

    public void Cancel(Actor participant)
    {
        if (!this.ActorsToCombat.TryGetValue(participant.RefId, out int runtimeId))
            return;
        var runtime = this.ActiveCombatSessions[runtimeId];
        runtime.MarkResolved();
    }

    public bool IsEngaged(Actor actor)
    {
        if (!this.ActorsToCombat.TryGetValue(actor.RefId, out var combat))
            return false;
        var runtime = this.ActiveCombatSessions[combat];
        if (runtime.IsResolved)
            return false;
        if (runtime.Second == actor && runtime.IsUnstarted)
            return false;
        return true;
    }

    public bool AreAvailableForNewCombat(Actor attacker, Actor defender)
        => !(this.ActorsToCombat.ContainsKey(attacker.RefId) || this.ActorsToCombat.ContainsKey(defender.RefId));

    public bool IsAvailableForNewCombat(Actor actor)
        => this.IsAvailableForNewCombat(actor.RefId);

    public bool IsAvailableForNewCombat(EntityRefId actorId)
    {
        if (this.ActorsToCombat.ContainsKey(actorId))
            return false;
        var avoidance = (int)this.World.Resolve<Actor>(actorId).Stats.Get(StatDefOf.AvoidCombatEncounter);
        if (this.World.Random.Roll100(avoidance))
            return false;
        return true;
    }

    public override void Tick()
    {
        if (!this.World.Net.IsServer)
            return;
        var toRemove = new List<CombatRuntime>(this.ActiveCombatSessions.Count);
        foreach (var combat in this.ActiveCombatSessions.Values)
        {
            if (combat.IsResolved)
                toRemove.Add(combat);
            else
                combat.Tick();
        }
        foreach (var combat in toRemove)
            this.World.Events.Post(new CombatEndEvent(this.World, combat.Id));
    }

    CombatRuntime RegisterCombat(Actor first, Actor second)
    {
        var id = this.NextId;
        var runtime = new CombatRuntime(id, first, second);
        this.ActiveCombatSessions.Add(id, runtime);
        this.ActorsToCombat.Add(first.RefId, id);
        this.ActorsToCombat.Add(second.RefId, id);
        ((FrontierManager)this.World.Space).Notifier.Notify(); // HACK
        return runtime;
    }

    void UnregisterCombat(int combatId)
    {
        var combat = this.ActiveCombatSessions[combatId];
        this.ActiveCombatSessions.Remove(combat.Id);
        this.ActorsToCombat.Remove(combat.First.RefId);
        this.ActorsToCombat.Remove(combat.Second.RefId);
        ((FrontierManager)this.World.Space).Notifier.Notify(); // HACK
    }

    internal CombatRuntime? Get(Actor actor)
    {
        if (!this.ActorsToCombat.TryGetValue(actor.RefId, out var combatId))
            return null;
        return this.ActiveCombatSessions[combatId];
    }

    internal CombatRuntime? Get(EntityRefId actorId)
    {
        if (!this.ActorsToCombat.TryGetValue(actorId, out var combatId))
            return null;
        return this.ActiveCombatSessions[combatId];
    }

    internal bool TryGet(Actor actor, out CombatRuntime? runtime)
    {
        if (!this.ActorsToCombat.TryGetValue(actor.RefId, out var combatId))
        {
            runtime = null;
            return false;
        }
        runtime = this.ActiveCombatSessions[combatId];
        return true;
    }
}
