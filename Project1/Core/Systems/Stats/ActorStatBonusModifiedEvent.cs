using Project1.Core.Events;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework.Serialization;

namespace Project1.Core.Entities.Stats;

[NetworkReplicate]
record struct ActorStatBonusModifiedEvent(StaticWorld World, EntityRefId ActorId, StatDef StatDef, float Delta)
    : IWorldEvent, ISerializableNewNew<ActorStatBonusModifiedEvent>
{
    public static ActorStatBonusModifiedEvent Create(IDataReader r)
    {
        return new()
        {
            ActorId = r.ReadEntityRefId(),
            StatDef = r.ReadDef<StatDef>(),
            Delta = r.ReadSingle()
        };
    }

    public readonly IDataWriter Write(IDataWriter w)
    => w.Write(ActorId)
        .Write(StatDef)
        .Write(Delta);
}
