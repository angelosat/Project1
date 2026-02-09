using Project1.Framework.Events;

namespace Project1.Core.Rooms
{
    internal record struct RoomUpdatedEvent(Room Room) : IEventPayload { }
}
