using Project1.Core.Base;

namespace Project1.Core.Net.Simulation
{
    public record struct PlayerChangedSpeedEvent(int Speed) : IEventPayload { }
}
