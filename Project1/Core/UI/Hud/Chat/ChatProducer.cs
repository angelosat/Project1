using Project1.Core.Networking;
using Project1.Core.Towns;
using Project1.Framework;

namespace Project1.Core.UI.Hud.Chat
{
    [EnsureStaticCtorCall]
    internal static class TownNotifications
    {
        static TownNotifications()
        {
            Registry.MapEventHooksClient.Register<MemberAddedEvent>(e
                => Client.Instance.ChatService.Post(ChatSource.Empty, $"{e.Actor.Name} joined the town!"));
            Registry.MapEventHooksClient.Register<MemberRemovedEvent>(e
               => Client.Instance.ChatService.Post(ChatSource.Empty, $"{e.Actor.Name} left the town!"));
        }
    }
}
