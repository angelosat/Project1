using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Effects;
using System;

namespace Project1.Core.Systems.Magic;

public sealed class Effect_TownTeleport : EntityEffectWorker
{
    public override EffectDef Def => EffectDefOf.TownTeleport;

    protected override void OnStart(Actor actor, EntityEffectWrapper wrapper)
    {
        if (actor.IsSpawned)
            throw new InvalidOperationException();
        var map = actor.Net.World.MainMap;
        var town = map.Town;
        if (!town.Waypoint.HasValue)
            throw new InvalidOperationException();
        var waypoint = town.Waypoint.Value;
        map.Spawn(actor, waypoint.Above, Vector3.Zero);
        map.Events.Post(new SpellCastEvent(actor, actor, SpellDefOf.Teleporting));
    }
    internal override string Label(Def target)
        => EffectDefOf.TownTeleport.LabelReadable;
}
