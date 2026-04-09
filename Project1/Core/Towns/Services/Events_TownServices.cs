using Project1.Core.Simulation;
using Project1.Framework.Events;

namespace Project1.Core.Towns.Services;

internal record struct TownServiceRequestUpdatedEvent(MapBase Map, TownServiceRequest Request) : IEventPayload;
