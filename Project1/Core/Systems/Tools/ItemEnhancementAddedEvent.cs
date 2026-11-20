using Project1.Core.Entities.Stats;
using Project1.Core.Events;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework.Serialization;

namespace Project1.Core.Systems.Tools;

[NetworkReplicate]
internal record struct ItemEnhancementAddedEvent(StaticWorld World,
                                                 EntityRefId ItemId,
                                                 StatDef StatDef,
                                                 float Value) 
    : IWorldEvent, ISerializableNewNew<ItemEnhancementAddedEvent>
{
    public static ItemEnhancementAddedEvent Create(IDataReader r)
    {
        return new()
        {
            ItemId = r.ReadEntityRefId(),
            StatDef = r.ReadDef<StatDef>(),
            Value = r.ReadSingle()
        };
    }

    public readonly IDataWriter Write(IDataWriter w)
    {
        w.Write(ItemId);
        w.Write(StatDef);
        w.Write(Value);
        return w;
    }
}
