using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Events;
using Project1.Core.Simulation;
using Project1.Framework;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.Systems.ItemRoles;

internal sealed class ItemEvaluationManager : EventOwner
{
    public Actor Actor;

    protected override StaticWorld World => field ??= (StaticWorld)this.Actor.World;
    protected override MapBase Map => this.Actor.Map;

    Entity Resolve(EntityRefId id)
        => this.Actor.World.Resolve(id);

    internal bool IsScanning => this.PendingItemQueue.Count > 0;

    readonly Queue<EntityRefId> PendingItemQueue = [];

    readonly HashSet<EntityRefId> QueuedItems = [];

    internal override EventHandlerDeclarations MapEventHandlers => field ??= new(
        new EventRegistration<EntitySpawnedEvent>(OnEntitySpawned)
        );

    public ItemEvaluationManager(Actor actor)
    {
        this.Actor = actor;
    }

    internal bool TryEnqueue(Entity item)
    {
        if (this.Actor.AI.State.Knowledge.TryQuery(item, out _))
        {
            $"{this.Actor} tried to enqueue already known item {item}".ToConsole();
            return false;
        }
        if (this.QueuedItems.Contains(item.RefId))
            return false;
        if (!this.QueuedItems.Add(item.RefId))
            return false;
        this.PendingItemQueue.Enqueue(item.RefId);
        return true;
    }

    public Entity? Dequeue()
    {
        if (this.PendingItemQueue.Count == 0)
            return null;
        var refId = this.PendingItemQueue.Dequeue();
        this.QueuedItems.Remove(refId);
        var entity = this.Resolve(refId);
        return entity;
    }

    protected override void OnAttachedToMapInt()
    {
        this.ScanMap();
    }

    private void ScanMap()
    {
        foreach (var i in this.Map.Entities)
            this.TryEnqueue(i);
    }

    private void OnEntitySpawned(EntitySpawnedEvent e)
    {
        this.TryEnqueue(e.Entity);
    }

    protected override void OnDespawnInt(MapBase oldMap)
    {
        this.PendingItemQueue.Clear();
        this.QueuedItems.Clear();
    }
    protected override void OnAttachedToWorldInt()
    {
        this.InitialInventoryScan();
    }

    private void InitialInventoryScan()
    {
        if (this.Actor.Net?.IsClient ?? true)
            return;
        var items = this.Actor.Inventory.GetItems();
        foreach (var item in items)
            this.TryEnqueue(item);
    }
}
