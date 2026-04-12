using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Events;

#nullable enable

namespace Project1.Core.Towns.Services.Shops;
internal record struct ShopTransactionUpdatedEvent(MapBase Map, ServiceRequest Transaction) : IEventPayload { }
internal record struct TownServiceCompleteEvent(MapBase Map, ServiceRequest Transaction) : IEventPayload { }
public sealed class ServiceRequest_Shop : ServiceRequest
{
    public EntityRefId Item { get; private set; }
    double TicksRemaining = Ticks.FromHours(1);
    internal override TownServiceDef Service => TownServiceDefOf.Buying;
    public ServiceRequest_Shop() { }
    public ServiceRequest_Shop(Actor buyer, Entity item, int price, IntVec3 counter) : base(buyer, price, counter)
    {
        this.Item = item.RefId;
    }
    public bool TimedOut => this.TicksRemaining <= 0;

    internal void Tick()
    {
        if (this.TicksRemaining <= 0)
            return;
        this.TicksRemaining--;
    }
    internal void RefreshTimer()
        => this.TicksRemaining = Ticks.FromHours(1);
}
