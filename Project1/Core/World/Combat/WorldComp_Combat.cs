using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.World.Frontiers;
using Project1.Framework.Events;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.World.Combat;

internal sealed class WorldComp_Combat(WorldBase world) : WorldComp(world)
{
    int NextId => ++field;

    readonly Dictionary<int, CombatRuntime> ActiveCombatSessions = [];

    readonly Dictionary<EntityRefId, int> ActorsToCombat = [];

    public void Engage(Actor instigator, Actor target)
    {
        var id = this.NextId;
        //var runtime = new CombatRuntime(id, first, second);
        this.World.Actions.Interrupt(target);
        this.RegisterCombat(id, instigator, target);//, runtime);
    }

    public bool IsEngaged(Actor actor)
    {
        if (!this.ActorsToCombat.TryGetValue(actor.RefId, out var combat))
            return false;
        if (this.ActiveCombatSessions[combat].IsResolved)
            return false;
        return true;
    }

    internal override void Tick()
    {
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

    internal void RegisterCombat(int id, Actor first, Actor second)
    {
        var runtime = new CombatRuntime(id, first, second);
        this.ActiveCombatSessions.Add(id, runtime);
        this.ActorsToCombat.Add(first.RefId, id);
        this.ActorsToCombat.Add(second.RefId, id);
        this.World.Events.Post(new CombatBeginEvent(id, first, second));
        ((FrontierManager)this.World.Space).Notifier.Notify();
    }

    internal void UnregisterCombat(CombatRuntime combat)
    {
        this.ActiveCombatSessions.Remove(combat.Id);
        this.ActorsToCombat.Remove(combat.First.RefId);
        this.ActorsToCombat.Remove(combat.Second.RefId);
        this.World.Events.Post(new CombatEndEvent(combat.Id));
        ((FrontierManager)this.World.Space).Notifier.Notify();
    }

    internal CombatRuntime? Get(Actor actor)
    {
        if (!this.ActorsToCombat.TryGetValue(actor.RefId, out var combatId))
            return null;
        return this.ActiveCombatSessions[combatId];
    }
}

internal record struct CombatBeginEvent(int Id, Actor Attacker, Actor Defender) : IEventPayload { }
internal record struct CombatEndEvent(int Id) : IEventPayload { }
