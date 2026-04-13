using Project1.Core.Simulation;
using Project1.Framework.Events;

namespace Project1.Core.Towns.Services;

internal record struct TownServiceRequestUpdatedEvent(MapBase Map, ServiceRequest Request) : IEventPayload;

internal record struct PlayerAssignedServiceToCounterEvent(BlockShopComp Comp, TownServiceDef Service) : IEventPayload;
internal record struct CounterServiceChangedEvent(BlockShopComp Comp, TownServiceDef OldService) : IEventPayload;
