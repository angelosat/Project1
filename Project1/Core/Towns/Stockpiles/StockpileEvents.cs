using Project1.Framework.Events;

namespace Project1.Core.Towns.Stockpiles
{
    internal record struct StockpileUpdatedEvent(Stockpile Stockpile) : IEventPayload { }
}
