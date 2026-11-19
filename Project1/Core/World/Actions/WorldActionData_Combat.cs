using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Framework.Serialization;

namespace Project1.Core.World.Actions;

internal sealed record WorldActionData_Combat(Actor Actor) : WorldActionData(Actor)
{
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
