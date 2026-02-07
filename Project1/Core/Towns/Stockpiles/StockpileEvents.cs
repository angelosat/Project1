using Project1.Core.Base;

namespace Project1.Core.Towns.Stockpiles
{
    internal record struct StockpileUpdatedEvent(Stockpile Stockpile) : IEventPayload { }
}
