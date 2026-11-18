using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Events;
using Project1.Core.Simulation;
using Project1.Framework;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.Systems.ItemRoles;

internal sealed class ItemEvaluationManager(Actor actor) : EventOwner
{
    public Actor Actor = actor;

    protected override StaticWorld World => field ??= (StaticWorld)this.Actor.World;

    Entity Resolve(EntityRefId id)
        => this.Actor.World.Resolve(id);

    internal bool IsScanning => this.PendingItemQueue.Count > 0;

    readonly Queue<EntityRefId> PendingItemQueue = [];

    readonly HashSet<EntityRefId> QueuedItems = [];

    internal bool TryEnqueue(Entity item)
    {
        if (this.Actor.AI.State.Knowledge.TryQuery(item, out _))
        {
            $"{this.Actor} tried to enqueue already known item {item}".ToConsole();
            return false;
        }
        if (this.QueuedItems.Contains(item.RefId))
            return false;
        //if (this.IsIgnored(item))
        //    return false;
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

    internal void OnSpawn(MapBase newMap)
    {
        foreach (var i in newMap.Entities)
            this.TryEnqueue(i);
        newMap.Events.ListenTo<EntitySpawnedEvent>(OnEntitySpawned);
    }

    internal void OnDispose()
        => this.Actor.World.Events.Unsubscribe(this);

    private void OnEntitySpawned(EntitySpawnedEvent e)
    {
        this.TryEnqueue(e.Entity);
    }

    public void OnDespawn(MapBase oldMap)
    {
        this.PendingItemQueue.Clear();
        this.QueuedItems.Clear();
        oldMap.Events.Unsubscribe(this);
    }

    internal void OnAttachedToWorld()
    {
        this.InitialInventoryScan();
    }

    internal void PostMapLoad()
    {
        this.OnSpawn(this.Actor.Map);
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
