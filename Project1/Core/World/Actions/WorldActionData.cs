using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.World.Frontiers;
using Project1.Framework.Serialization;

namespace Project1.Core.World.Actions;

internal abstract record WorldActionData(Actor Actor) : ISerializable
{
    internal abstract WorldActionDef Def { get; }

    internal FrontierRuntime Frontier => field ??= this.Actor.Frontier;

    internal StaticWorld World => field ??= (StaticWorld)this.Actor.World;

    internal void Resolve()
        => this.Def.Worker.Resolve(this);

    public virtual ISerializable Read(IDataReader r) => this;

    public virtual void Write(IDataWriter w) { }
}

internal record WorldActionData_Default(Actor Actor, WorldActionDef ActionDef) : WorldActionData(Actor)
{
    internal override WorldActionDef Def => this.ActionDef;
}
