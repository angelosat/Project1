using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using System.Collections.Generic;

namespace Project1.Core.Systems.CombatOverworld;

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
