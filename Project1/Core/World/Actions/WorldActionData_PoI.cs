using Project1.Core.Entities.Actors;
using Project1.Core.World.Frontiers;
using Project1.Core.World.PointsOfInterest;
using Project1.Framework.Serialization;

namespace Project1.Core.World.Actions;

internal sealed record WorldActionData_PoI(FrontierRuntime Frontier, Actor Actor) : WorldActionData(Frontier, Actor)
{
    internal PoIRuntime PoI => field ??= this.Frontier.PoIs[this.PoIId];
    internal required int PoIId;

    public override void Write(IDataWriter w)
    {
        w.Write(this.PoIId);
    }
    public override ISerializable Read(IDataReader r)
    {
        this.PoIId = r.ReadInt32();
        return this;
    }
}
