using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Framework;

namespace Project1.Core.UI.Hud.Chat
{
    [EnsureStaticCtorCall]
    internal static class ChatPackets
    {
        internal static readonly PacketId _pChat = Registry.PacketHandlers.Register(ReceivePlayerChat);

        static ChatPackets()
        {
            Registry.PlayerInputEventHooks.Register<PlayerChatEvent>(HandlePlayerChatEvent);
        }

        private static void HandlePlayerChatEvent(PlayerChatEvent e)
        {
            if (Ingame.Net.IsServer)
                Ingame.Net.ChatService.Post(ChatSource.Player(Ingame.Net.CurrentPlayer), e.Text);
            SendPlayerChat(Ingame.Net, Ingame.Net.CurrentPlayer, e.Text);
        }
        static void SendPlayerChat(NetEndpoint net, PlayerData player, string text)
        {
            var w = net.BeginPacketImmediate(_pChat);
            w.Write(player.ID);
            w.WriteASCII(text);
        }

        private static void ReceivePlayerChat(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var playerid = (PlayerId)r.ReadInt32();
            var player = endpoint.GetPlayer(playerid);
            var text = r.ReadASCII();
            endpoint.ChatService.Post(ChatSource.Player(player), text);
            if (endpoint.IsServer)
                SendPlayerChat(endpoint, player, text);
        }
    }
}
