using Project1.Core.Entities.Actors;
using Project1.Core.World.WorldAreas;
using System.Collections.Generic;
using System.Threading;

namespace Project1.Core.World.PointsOfInterest;

internal class PoIRuntime
{
    PoIDef Def;

    readonly HashSet<EntityRefId>
        ActorsActive = [],
        ActorsIncapacitated = [],
        Loot = [];

    readonly PoISecret[] Secrets, SecretsDiscovered, SecretsUndiscovered;// = [];

    public readonly FrontierRuntime Frontier;
    public readonly int Size;
    public int SecretsTotal, SecretsDiscoveredCount;
    public int SecretsUndiscoveredCount => SecretsTotal - SecretsDiscoveredCount;

    public int ExplorationTicksRequired { get; internal set; }

    public PoIRuntime(PoIDef def, int size)
    {
        Def = def;
        Size = size;
        this.ExplorationTicksRequired = size * Ticks.FromHours(1);
        SecretsTotal = size;
        Secrets = new PoISecret[size];
        for (int i = 0; i < size; i++)
            Secrets[i] = new(Frontier.World.Random.Next(100));
    }

    internal bool IsOpportunity(float previousPercentage, float nextPercentage)
    {
        var prev = previousPercentage * 100;
        var next = nextPercentage * 100;
        foreach (var secret in Secrets)
            if (prev < secret.Keyframe && secret.Keyframe <= next)
                return true;
        return false;
    }

    internal bool TryTriggerOpportunity(Actor actor, float previousPercentage, float nextPercentage)
    {
        if(!this.IsOpportunity(previousPercentage, nextPercentage)) 
            return false;
        return true;
    }

    public bool Contains(Actor actor)
        => ActorsActive.Contains(actor.RefId);

    internal void Add(Actor actor)
        => ActorsActive.Add(actor.RefId);

    public IEnumerable<int> RollTimeline()
    {
        for (var i = 0; i < SecretsUndiscoveredCount; i++)
            yield return Frontier.World.Random.Next(100);
    }
}
