using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Framework.Events;
using System.Collections.Generic;

namespace Project1.Core.Systems.Ownership;

internal record struct ItemOwnerChangedEvent(Entity Item, EntityRefId OldOwner) : IEventPayload;
internal class WorldComp_Ownership : WorldComp
{
    readonly Dictionary<EntityRefId, HashSet<EntityRefId>> _possessionsByActor = [];
    public WorldComp_Ownership(WorldBase world) : base(world)
    {
        world.Events.ListenTo<ItemOwnerChangedEvent>(HandleItemOwnerChanged);
        world.Events.ListenTo<EntityDisposedEvent>(HandleEntityDisposed);
    }

    private void HandleEntityDisposed(EntityDisposedEvent e)
    {
        var item = e.Entity;
        var ownerid = item.OwnerId;
        if (ownerid == EntityRefId.Null)
            return;
        this.UnregisterPossession(item.RefId, ownerid);
    }

    private void HandleItemOwnerChanged(ItemOwnerChangedEvent e)
    {
        var item = e.Item.RefId;
        var old = e.OldOwner;
        UnregisterPossession(item, old);
        var @new = e.Item.OwnerId;
        if (@new == EntityRefId.Null)
            return;
        this.RegisterPossession(item, @new);
    }
    internal IReadOnlySet<EntityRefId> Get(Actor actor)
        => this._possessionsByActor.TryGetValue(actor.RefId, out var list) ? list : [];
    internal override void Scan(Entity entity)
    {
        var owner = entity.OwnerId;
        if (owner == EntityRefId.Null)
            return;
        RegisterPossession(entity.RefId, owner);
    }

    private void RegisterPossession(EntityRefId entity, EntityRefId actor)
    {
        if (!this._possessionsByActor.TryGetValue(actor, out var list))
            this._possessionsByActor[actor] = list = [];
        list.Add(entity);
    }

    private void UnregisterPossession(EntityRefId item, EntityRefId actor)
    {
        if (this._possessionsByActor.TryGetValue(actor, out var list))
        {
            list.Remove(item);
            if (list.Count == 0)
                this._possessionsByActor.Remove(actor);
        }
    }
}
