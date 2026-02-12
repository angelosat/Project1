using Project1.Framework.Events;

namespace Project1.Core.Networking
{
    internal record struct IncomingConnectionEvent(PlayerData Player, bool Connected) : IEventPayload { }
    internal record struct ServerConnectionAcceptedEvent : IEventPayload { }
    internal record struct PlayerConnectedEvent(PlayerData Player, bool Connected) : IEventPayload { }
}
