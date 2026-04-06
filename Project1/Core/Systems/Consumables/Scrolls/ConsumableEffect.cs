using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Framework.Events;
using System;

namespace Project1.Core.Systems.Consumables.Scrolls;


public abstract class ConsumableEffect
{
    internal abstract void Execute(Actor actor);
}

internal sealed class ConsumableEffect_TownScroll : ConsumableEffect
{
    internal override void Execute(Actor actor)
    {
        if (actor.IsSpawned)
            throw new InvalidOperationException();
        var town = actor.Net.Map.Town;
        if (!town.Waypoint.HasValue)
            throw new InvalidOperationException();
        var waypoint = town.Waypoint.Value;
        town.Map.Spawn(actor, waypoint.Above, Vector3.Zero);
        town.Map.Events.Post(new EntityTeleportedEvent(actor));
    }
}

internal sealed class ConsumableEffect_Food : ConsumableEffect
{
    internal override void Execute(Actor actor)
    {
        //if (actor.IsSpawned)
        //    throw new InvalidOperationException();
        //var town = actor.Net.Map.Town;
        //if (!town.Waypoint.HasValue)
        //    throw new InvalidOperationException();
        //var waypoint = town.Waypoint.Value;
        //town.Map.Spawn(actor, waypoint.Above, Vector3.Zero);
        //town.Map.Events.Post(new EntityTeleportedEvent(actor));
    }
}


internal record struct EntityTeleportedEvent(Entity Entity) : IEventPayload;