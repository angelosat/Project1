using Project1.Core.Entities.Actors;
using Project1.Core.World.Frontiers;
using Project1.Framework.Serialization;

namespace Project1.Core.World.Actions;

internal class WorldActionData(FrontierRuntime frontier, Actor actor) : ISerializable
{
    internal readonly FrontierRuntime Frontier = frontier;

    internal readonly Actor Actor = actor;

    public virtual ISerializable Read(IDataReader r) { return this; }

    public virtual void Write(IDataWriter w) 
    { 

    }
}
