using Project1.Core.Simulation;
using Project1.Framework.Events;
using System;
using System.Linq;

namespace Project1.Core.Towns.Services.Shops
{
    internal record struct TransactionStartedEvent(MapBase Map, ServiceRequest_Shop Transaction) : IEventPayload;
}
