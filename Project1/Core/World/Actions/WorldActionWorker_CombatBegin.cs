using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Core.World.Frontiers;
using Project1.Framework.Serialization;

namespace Project1.Core.World.Actions;

internal class WorldActionWorker_CombatBegin : WorldActionWorker
{
    internal override void Resolve(WorldActionData data)
    {
        var typed = data as WorldActionData_Combat;
        var world = (StaticWorld)data.Actor.World;
        var other = world.Get<Actor>(typed.Other);
        world.Combat.Engage(data.Actor, other);
    }
}

internal sealed record WorldActionData_Combat(FrontierRuntime Frontier, Actor Actor) : WorldActionData(Frontier, Actor)
{
    internal required EntityRefId Other;

    public override void Write(IDataWriter w)
    {
        w.Write(this.Other);
    }

    public override ISerializable Read(IDataReader r)
    {
        this.Other = r.ReadEntityRefId();
        return this;
    }
}
