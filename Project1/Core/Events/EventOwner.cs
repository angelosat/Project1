using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

#nullable enable

namespace Project1.Core.Events;

public abstract class EventOwner : Inspectable
{
    private EventOwner? Parent;

    private readonly List<EventOwner> Children = [];

    protected abstract StaticWorld World { get; }

    protected abstract MapBase Map { get; }

    internal virtual EventHandlerDeclarations MapEventHandlers { get; } = new();

    internal virtual EventHandlerDeclarations WorldEventHandlers { get; } = new();

    protected void AddChild(EventOwner child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (child == this)
            throw new InvalidOperationException("An EventOwner cannot own itself.");

        if (child.Parent is not null)
            throw new InvalidOperationException("EventOwner already has a parent.");

        for (var ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
        {
            if (ancestor == child)
                throw new InvalidOperationException("Adding this child would create a cycle.");
        }

        child.Parent = this;
        this.Children.Add(child);
    }

    public void OnAttachedToMap()
    {
        this.OnAttachedToMapInt();
        this.MapEventHandlers.Subscribe(this.Map.Events);
        foreach (var child in this.Children)
            child.OnAttachedToMap();
    }

    public void OnAttachedToWorld()
    {
        this.OnAttachedToWorldInt();
        this.WorldEventHandlers.Subscribe(this.World.Events);
        foreach (var child in this.Children)
            child.OnAttachedToWorld();
    }

    protected virtual void OnAttachedToWorldInt() { }
    protected virtual void OnAttachedToMapInt() { }

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

internal interface IEventHandlerDeclaration
{
    void Subscribe(EventBus bus);
}

internal sealed record EventRegistration<T>(Action<T> Handler)
    : IEventHandlerDeclaration
    where T : IEventPayload
{
    public void Subscribe(EventBus bus)
    {
        bus.ListenTo(this.Handler);
    }
}

internal sealed class EventHandlerDeclarations(params IEventHandlerDeclaration[] items)
{
    private readonly ImmutableArray<IEventHandlerDeclaration> _items = [.. items];

    public void Subscribe(EventBus bus)
    {
        foreach (var item in this._items)
            item.Subscribe(bus);
    }
}