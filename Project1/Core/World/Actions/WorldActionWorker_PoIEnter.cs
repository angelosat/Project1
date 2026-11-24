using Project1.Core.Entities.Actors;
using Project1.Core.World.Frontiers;
using Project1.Core.World.PointsOfInterest;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.World.Actions;

internal class WorldActionWorker_PoIEnter : WorldActionWorker
{
    sealed record Data(Actor Actor) : WorldActionData(Actor)
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

    static public (WorldActionDef, WorldActionData) Create(Actor actor, int PoiId)
        => (WorldActionDefOf.EnterPoI, new Data(actor) { PoIId = PoiId });

    internal override Type DataType => typeof(Data);

    static Data Typed(WorldActionData data) => (Data)data;

    internal override void Resolve(WorldActionData data)
    {
        var typed = (Data)data;
        var id = typed.PoIId;
        ((FrontierManager)data.Frontier.World.Space).ActorPoIEnter(data.Actor, data.Frontier, id);
        //var poi = data.Frontier.PoIs[id];
        ////var poi = typed.PoI;
        //poi.Add(data.Actor);
    }

    internal override string GetStatusText(WorldActionRuntime runtime)
        => $"Entering {Typed(runtime.Data).PoI.Def.LabelReadable}";
}
