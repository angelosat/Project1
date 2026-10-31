using Project1.Core.Entities.Actors;
using Project1.Core.World.Frontiers;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using System.Collections.Generic;

namespace Project1.Core.World.Rooms;

public abstract class WorldRoom
{
    protected readonly HashSet<EntityRefId> ActorsActive = [];

    public IReadOnlyList<EntityRefId> ActorsActiveNew => (IReadOnlyList<EntityRefId>)this.ActorsActive;

    public int OccupantCount => this.ActorsActive.Count;

    public ProgressFloat Exploration = new(0, 100, 0);

    public void Explore()
        => this.Exploration.ApplyDelta(1);
}

public record struct WorldExplorationEvent(FrontierRuntime Frontier, Actor Actor) : IEventPayload;
