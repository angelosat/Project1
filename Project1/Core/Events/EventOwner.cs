using Project1.Core.Simulation;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Events;

public abstract class EventOwner : Inspectable
{
    private readonly List<EventOwner> Children = [];

    protected abstract StaticWorld World { get; }

    protected void AddChild(EventOwner child)
    {
        this.Children.Add(child);
    }

    public void OnDispose()
    {
        this.World.Events.Unsubscribe(this);

        foreach (var child in this.Children)
            child.OnDispose();

        this.OnDisposeInt();
    }

    public void OnDespawn(MapBase oldMap)
    {
        oldMap.Events.Unsubscribe(this);

        foreach (var child in this.Children)
            child.OnDespawn(oldMap);

        this.OnDespawnInt(oldMap);
    }

    protected virtual void OnDisposeInt() { }
    protected virtual void OnDespawnInt(MapBase oldMap) { }
}
