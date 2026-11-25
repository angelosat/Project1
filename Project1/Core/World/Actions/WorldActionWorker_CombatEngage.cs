using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.World.Actions;

internal sealed record WorldActionData_CombatEngage(Actor Actor) : WorldActionData(Actor)
{
    internal override WorldActionDef Def => WorldActionDefOf.Combat;

    internal required EntityRefId TargetId;

    public override void Write(IDataWriter w)
    {
        w.Write(TargetId);
    }

    public override ISerializable Read(IDataReader r)
    {
        TargetId = r.ReadEntityRefId();
        return this;
    }
}

internal sealed class WorldActionWorker_CombatEngage : WorldActionWorker
{
    internal override Type DataType => typeof(WorldActionData_CombatEngage);

    public static (WorldActionDef, WorldActionData) Create(Actor actor, EntityRefId targetId)
        => (WorldActionDefOf.Combat, new WorldActionData_CombatEngage(actor) { TargetId = targetId });

    internal override void Resolve(WorldActionData data)
    {
        var typed = data as WorldActionData_CombatEngage;
        var world = (StaticWorld)data.Actor.World;
        var other = world.Resolve<Actor>(typed.TargetId);
        world.Combat.Engage(data.Actor, other);
    }
}
