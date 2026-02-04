using Project1.Framework.Base;

namespace Project1.Framework.StaticMaps.Packets
{
    public record struct PlayerChangedSpeedEvent(int Speed) : IEventPayload { }
}
