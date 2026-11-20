using Project1.Core.Events;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework.Serialization;

namespace Project1.Core.Systems.Quality;

[NetworkReplicate]
internal record struct ItemRarityUpgradedEvent(StaticWorld World,
                                               EntityRefId ItemId)
    : IWorldEvent, ISerializableNewNew<ItemRarityUpgradedEvent>
{
    public static ItemRarityUpgradedEvent Create(IDataReader r)
    {
        return new()
        {
            ItemId = r.ReadEntityRefId()
        };
    }

    public readonly IDataWriter Write(IDataWriter w)
    {
        w.Write(ItemId);
        return w;
    }
}