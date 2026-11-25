using Project1.Core.Entities.Actors;
using Project1.Core.World.Frontiers;
using Project1.Core.World.PointsOfInterest;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.World.Actions;

internal sealed record WorldActionData_PoiEnter(Actor Actor) : WorldActionData(Actor)
{
    internal override WorldActionDef Def => WorldActionDefOf.EnterPoI;

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

internal class WorldActionWorker_PoIEnter : WorldActionWorker
{
    internal override Type DataType => typeof(WorldActionData_PoiEnter);

    static WorldActionData_PoiEnter Typed(WorldActionData data) => (WorldActionData_PoiEnter)data;

    internal override void Resolve(WorldActionData data)
    {
        var typed = (WorldActionData_PoiEnter)data;
        var id = typed.PoIId;
        ((FrontierManager)data.Frontier.World.Space).ActorPoIEnter(data.Actor, data.Frontier, id);
    }

    internal override string GetStatusText(WorldActionRuntime runtime)
        => $"Entering {Typed(runtime.Data).PoI.Def.LabelReadable}";
}
