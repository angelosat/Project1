using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public class LogEventArgs : EventArgs
    {
        public Log.Entry Entry;
        public LogEventArgs(Log.Entry entry)
        {
            Entry = entry;
        }
    }
    public partial class Log
    {
        static readonly Queue<Entry> EntryQueue = new();
        public enum EntryTypes
        {
            Damage, Death, Equip, Unequip, Default, TooHeavy, Buff, Debuff, Skill, CellChanged,
            /// <summary>
            /// <para>arg0: object</para>
            /// <para>arg1: text</para>
            /// </summary>
            Chat,
            System, Dialogue, DialogueOption, DialogueEnd, Jobs,
            ChatPlayer,
            Warning,
            Error,
            Network,
            Notification
        }
        static Log _instance;
        static public Log Instance => _instance ??= new Log();
        
        static Dictionary<EntryTypes, List<EventHandler<LogEventArgs>>> Events = new();

        List<Entry> Entries;
        void OnEntryAdded(LogEventArgs e)
        {
            List<EventHandler<LogEventArgs>> list;
            if (Events.TryGetValue(e.Entry.Type, out list))
                foreach (EventHandler<LogEventArgs> handler in list)
                    handler.Invoke(this, e);
        }

        static public void Update()
        {
            while (EntryQueue.Count > 0)
            {
                Write(EntryQueue.Dequeue());
            }
        }

        public Log()
        {
            Entries = new List<Entry>();
        }
        static public void Enqueue(EntryTypes type, params object[] values)
        {
            EntryQueue.Enqueue(new Entry(type, values));
        }
        static void Write(Entry entry)
        {
            Instance.OnEntryAdded(new LogEventArgs(entry));
            if (entry.ToString().Length == 0)
                return;
            Instance.Entries.Add(entry);
            LogWindow.Write(new LogEventArgs(entry));
        }
        static void Write(EntryTypes type, params object[] values)
        {
            Entry entry = new Entry(type, values);
            Instance.OnEntryAdded(new LogEventArgs(entry));
            if (entry.ToString().Length == 0)
                return;
            Instance.Entries.Add(entry);
        }
        readonly static StreamWriter LogFile = new("Log.txt", false) { AutoFlush = true };
     
        public static void WriteToFile(string text)
        {
            var textStamped = $"[{DateTime.Now}] {text}";
            textStamped.ToConsole();
            LogFile.WriteLine(textStamped);
        }
      
       
        static void WriteHelp()
        {
            Log.Write(EntryTypes.Default,
                            @"
List of available commands:
/querycell
/querychunk
/give
/set
/updatelight
/updatethumb
/refreshstats
/help
");
        }

        public override string ToString()
        {
            bool first = true;
            string text = "";
            foreach (Entry entry in Entries)
            {
                if (!first)
                    text += "\n";
                text += entry.ToString();
            }
            return text;
        }

        internal static void Command(string gotText)
        {
            List<string> words = new List<string>();
            string[] text = gotText.Split('\'');
            for (int i = 0; i < text.Length; i++)
            {
                if (i % 2 == 0)
                    words.AddRange(text[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                else
                    words.Add(text[i]);
            }
            Queue<string> wordQueue = new Queue<string>(words);
            string msg = wordQueue.Dequeue();
            try
            {
                switch (msg)
                {
                    case "/set":
                        Set(wordQueue);
                        break;
                    case "/updatethumb":
                        UpdateMapThumpnail();
                        break;
                   
                    case "/help":
                        Log.WriteHelp();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception) { 
                Log.Write(EntryTypes.Default, "Catastrophic Failure!"); 
            }
        }

        private static void UpdateMapThumpnail()
        {
            string worldPath = @"/Saves/Worlds/" + Engine.Map.GetName() + "/";

            using Texture2D thumbnail = Engine.Map.GetThumbnail();
            using (FileStream stream = new(Directory.GetCurrentDirectory() + worldPath + "thumbnailSmall.png", FileMode.OpenOrCreate))
            {
                thumbnail.SaveAsPng(stream, thumbnail.Width, thumbnail.Height);
                stream.Close();
            }
            using (FileStream stream = new(Directory.GetCurrentDirectory() + worldPath + "thumbnailSmaller.png", FileMode.OpenOrCreate))
            {
                thumbnail.SaveAsPng(stream, thumbnail.Width / 2, thumbnail.Height / 2);
                stream.Close();
            }
            using (FileStream stream = new(Directory.GetCurrentDirectory() + worldPath + "thumbnailSmallest.png", FileMode.OpenOrCreate))
            {
                thumbnail.SaveAsPng(stream, thumbnail.Width / 4, thumbnail.Height / 4);
                stream.Close();
            }
        }

        private static void Set(Queue<string> words)
        {
            string setWhat = words.Dequeue();
            switch (setWhat)
            {
                case "ambience":
                    string colorName = words.Peek();
                    Color color;
                    try
                    {
                        color = (Color)typeof(Color).GetProperty(colorName).GetValue(null, null);
                        Engine.Map.SetAmbientColor(color);
                        break;
                    }
                    catch (Exception)
                    {
                    }

                    try
                    {
                        int r = int.Parse(words.Dequeue()),
                            g = int.Parse(words.Dequeue()),
                            b = int.Parse(words.Dequeue());
                        Engine.Map.SetAmbientColor(new Color(r, g, b));
                    }
                    catch (Exception)
                    {
                        Enqueue(EntryTypes.System, "Invalid color");
                    }
                    break;
               
                case "hour":
                case "time":
                    int t = int.Parse(words.Dequeue());
                    foreach (var ch in Engine.Map.GetActiveChunks())
                        ch.Value.LightCache.Clear();
                    Enqueue(EntryTypes.System, "Hour set to " + t);
                    break;
                
                default:
                    Log.Enqueue(EntryTypes.System, "Invalid command");
                    break;
            }
        }
        private static void _write(Entry entry)
        {
            var text = entry.ToString();
            var timeStamped = $"[{DateTime.Now:HH:MM:ss}] {text}";
            timeStamped.ToConsole();
            UIChat.Instance.Write(entry);
            DebugConsole.Write(text);

        }
        public static void Write(string text)
        {
            var timeStamped = $"[{DateTime.Now:HH:MM:ss}] {text}";
            timeStamped.ToConsole();
            UIChat.Instance.Write(text);
            DebugConsole.Write(text);
        }
        public static void Warning(string text)
        {
            _write(Entry.Warning(text));
        }
        public static void Error(string text)
        {
            _write(Entry.Error(text));
        }
        public static void System(string text)
        {
            _write(Entry.System(text));
        }
        internal static void Chat(PlayerData player, string txt)
        {
            _write(Entry.Chat(player, txt));
        }
        internal static void Network(INetEndpoint net, string txt)
        {
            _write(Entry.Network(net, txt));
        }
    }
}
