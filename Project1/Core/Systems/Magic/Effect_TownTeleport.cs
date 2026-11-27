using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Effects;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.Systems.Magic;

public sealed record EffectConfig_TownTeleport : EffectConfig
{
    public override EffectData CreateData()
        => new EffectData_TownTeleport();

    public override string GetString(float scaling)
        => $"Teleports back to town";
}

public sealed class EffectData_TownTeleport : EffectData<EffectConfig_TownTeleport>
{
    public override EffectDef Def => EffectDefOf.TownTeleport;

    public override ISerializable Read(IDataReader r)
        => this;

    public override void Write(IDataWriter w) { }
}

public sealed class Effect_TownTeleport : EntityEffectWorker<EffectData_TownTeleport>
{
    //protected override void OnPerform(Actor actor)
    //{
    //    if (actor.IsSpawned)
    //        throw new InvalidOperationException();
    //    var map = actor.Net.World.MainMap;
    //    var town = map.Town;
    //    if (!town.Waypoint.HasValue)
    //        throw new InvalidOperationException();
    //    var waypoint = town.Waypoint.Value;
    //    map.Spawn(actor, waypoint.Above, Vector3.Zero);
    //    map.Events.Post(new SpellCastEvent(actor, actor, SpellDefOf.Teleporting));
    //}
    protected override void OnStart(Actor actor, EffectData_TownTeleport dto)
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
    //internal override string Label(Def target)
    protected override string GetString(EffectData_TownTeleport dto)
        => dto.Def.LabelReadable;
}
