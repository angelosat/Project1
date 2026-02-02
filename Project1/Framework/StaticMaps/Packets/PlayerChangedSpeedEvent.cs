using Start_a_Town_;

namespace Project1.Framework.StaticMaps.Packets
{
    public record struct PlayerChangedSpeedEvent(int Speed) : IEventPayload { }
}
