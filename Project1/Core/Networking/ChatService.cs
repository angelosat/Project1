using Microsoft.Xna.Framework;
using Project1.Core.UI.Hud.Chat;
using System;
using System.Collections.Generic;

namespace Project1.Core.Networking
{
    internal record struct ChatEntry(DateTime TimeStamp, ChatSource Source, string Text) { }

    public readonly struct ChatSource
    {
        public string DisplayName { get; }
        public Color TextColor { get; }

        private ChatSource(string name, Color color)
        {
            DisplayName = name;
            TextColor = color;
        }

        public static readonly ChatSource System = new("SYSTEM", Color.Yellow);
        public static ChatSource Player(PlayerData player) => new(player.Name, Color.LightGray);
    }
    public class ChatService(NetEndpoint endpoint)
    {
        NetEndpoint Endpoint = endpoint;
       
        readonly Stack<ChatEntry> History = new(16);
        int HistoryIndex;
        
        public void Post(ChatSource source, string text)
        {
            var entry = new ChatEntry(DateTime.UtcNow, source, text);
            this.Endpoint.Events.Post(new ChatEvent(entry));
            this.History.Push(entry);
        }
    }
}
