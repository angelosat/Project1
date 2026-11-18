using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.World.Frontiers;
using Project1.Framework.Serialization;

namespace Project1.Core.World.Actions;

internal record WorldActionData(Actor Actor) : ISerializable
{
    internal FrontierRuntime Frontier => field ??= this.Actor.Frontier;

    internal StaticWorld World => field ??= (StaticWorld)this.Actor.World;
    public virtual ISerializable Read(IDataReader r) => this;

    public virtual void Write(IDataWriter w) { }
}
