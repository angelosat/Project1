using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Magic;
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
        var map = actor.Net.World.MainMap;
        var town = map.Town;
        if (!town.Waypoint.HasValue)
            throw new InvalidOperationException();
        var waypoint = town.Waypoint.Value;
        map.Spawn(actor, waypoint.Above, Vector3.Zero);
        map.Events.Post(new Events_Spells(actor, SpellDefOf.Teleporting));
    }
}

internal sealed class ConsumableEffect_Food : ConsumableEffect
{
    internal override void Execute(Actor actor)
    {

    }
}
