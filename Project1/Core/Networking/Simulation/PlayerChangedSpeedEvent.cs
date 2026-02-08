using Project1.Core.Base;

namespace Project1.Core.Networking.Simulation
{
    public record struct PlayerChangedSpeedEvent(int Speed) : IEventPayload { }
}
