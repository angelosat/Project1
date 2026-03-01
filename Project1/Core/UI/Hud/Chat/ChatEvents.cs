using Project1.Core.Networking;
using Project1.Framework.Events;

namespace Project1.Core.UI.Hud.Chat
{
    internal record struct PlayerChatEvent(string Text) : IEventPayload { }
    internal record struct ChatEvent(ChatEntry Entry) : IEventPayload { }
}
