using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.World.Rooms;
using Project1.Core.World.Frontiers;
using Project1.Framework.Events;
using System;
using System.Collections.Generic;

namespace Project1.Core.World.PointsOfInterest;

internal class PoIRuntime : WorldRoom
{
    public readonly PoIDef Def;

    readonly HashSet<EntityRefId>
        //ActorsActive = [],
        ActorsIncapacitated = [],
        Loot = [];

    PoISecret[] Secrets, SecretsDiscovered, SecretsUndiscovered;// = [];

    public readonly FrontierRuntime Frontier;
    public readonly int Size, Id;
    public bool IsDiscovered;
    public int SecretsTotal, SecretsDiscoveredCount;
    public int SecretsUndiscoveredCount => SecretsTotal - SecretsDiscoveredCount;

    public int ExplorationTicksRequired { get; internal set; }

    public PoIRuntime(FrontierRuntime frontier, int id, PoIDef def, int size)
    {
        this.Frontier = frontier;
        this.Id = id;
        this.Def = def;
        this.Size = size;
        this.ExplorationTicksRequired = size * Ticks.FromHours(1);
        this.SecretsTotal = size;
        this.Secrets = new PoISecret[size];
    }

    internal void Init(StaticWorld world)
    {
        this.RollTimeline(world);
    }

    private void RollTimeline(StaticWorld world)
    {
        this.Secrets = new PoISecret[this.Size];
        for (int i = 0; i < this.Size; i++)
            this.Secrets[i] = new(world.Random.Next(100));
    }

    internal void Discover() => this.IsDiscovered = true;

    internal bool IsOpportunity(float previousPercentage, float nextPercentage)
    {
        var prev = previousPercentage * 100;
        var next = nextPercentage * 100;
        foreach (var secret in Secrets)
            if (prev < secret.Keyframe && secret.Keyframe <= next)
                return true;
        return false;
    }

    internal bool TryEvent(Actor actor, float previousPercentage, float nextPercentage)
    {
        if(!this.IsOpportunity(previousPercentage, nextPercentage)) 
            return false;
        return true;
        throw new NotImplementedException();
    }

    public bool Contains(Actor actor)
        => this.ActorsActive.Contains(actor.RefId);

    internal void Add(Actor actor)
    { 
        this.ActorsActive.Add(actor.RefId);
        this.Frontier.World.Events.Post(new PoIActorAddedEvent(actor, this.Frontier.Def, this.Id));
    }

    internal void Remove(Actor actor)
        => this.ActorsActive.Remove(actor.RefId);

    //public IEnumerable<int> RollTimeline()
    //{
    //    for (var i = 0; i < SecretsUndiscoveredCount; i++)
    //        yield return Frontier.World.Random.Next(100);
    //}
}

internal record struct PoIAddedEvent(FrontierDef FrontierDef, PoIRuntime PoI) : IEventPayload;
internal record struct PoIRemovedEvent(int Id) : IEventPayload;
internal record struct PoIActorAddedEvent(Actor Actor, FrontierDef Frontier, int PoIId) : IEventPayload;
