using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Framework.Serialization;

namespace Project1.Core.World.Actions;

internal sealed record WorldActionData_Loot(Actor Actor) : WorldActionData(Actor)
{
    internal required EntityRefId[] Loot;

    public override void Write(IDataWriter w)
    {
        w.Write(Loot);
    }

    public override ISerializable Read(IDataReader r)
    {
        Loot = r.ReadArrayEntityRefId();
        return this;
    }
}
