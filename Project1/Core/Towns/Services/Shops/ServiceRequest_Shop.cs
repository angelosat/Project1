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
    double TicksRemaining = Ticks.FromHours(1);
    internal override TownServiceDef Service => TownServiceDefOf.Buying;
    public ServiceRequest_Shop() { }

    public ServiceRequest_Shop(Actor customer, Entity item, int price, IntVec3 counter) : base(customer, item, price, counter)
    {
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
