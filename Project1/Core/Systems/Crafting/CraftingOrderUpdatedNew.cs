using Project1.Core.Helpers;
using Project1.Framework.Events;
using Project1.Framework.Serialization;

namespace Project1.Core.Systems.Crafting;

record struct CraftingOrderUpdatedNew(MapId Map, /*EntityRefId Actor, */CraftingOrderId Order) : IEventPayload, ISerializableNewNew<CraftingOrderUpdatedNew>
{
    public static CraftingOrderUpdatedNew Create(IDataReader r)
        => new(r.ReadId<MapId>(), /*r.ReadId<EntityRefId>(),*/ r.ReadId<CraftingOrderId>());

    public IDataWriter Write(IDataWriter w)
    {
        w.Write(Map);
        //w.Write(Actor);
        w.Write(Order);
        return w;
    }
}
