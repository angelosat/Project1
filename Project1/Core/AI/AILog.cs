using Project1.Core.Entities.Actors;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Project1.Core.AI
{
    public class AILog
    {
        const int Capacity = 64;
        public readonly ObservableCollection<Entry> Inner = new();
        private Actor Owner;

        public AILog(Actor owner)
        {
            this.Owner = owner;
        }
      
        public Entry Write(string text)
        {
            var entry = new Entry(DateTime.Now, text);
            this.Inner.Add(entry);
            if (this.Inner.Count > Capacity)
                this.Inner.RemoveAt(0);
            this.Owner.World?.Events.Post(new AILogEntryEvent(this.Owner, text));
            return entry;
        }
        
        public List<Entry> GetEntries()
        {
            return this.Inner.ToList();
        }
        
        public class UI
        {
            static readonly Lazy<TableScrollableCompact<Entry>> EntriesGUI = new(()=> new TableScrollableCompact<Entry>()
                    .AddColumn(null, "Time", (int)UIManager.Font.MeasureString("HH:mm:ss").X, (e) => new Label(e.Time.ToString("HH:mm:ss")), 0)
                    .AddColumn(null, "Description", 1000, (e) => new GroupBox().AddControlsLineWrap(Label.ParseNew(e.Text)), 0));

            static public Control GetGUI(Actor actor)
            {
                return EntriesGUI.Value.Bind(actor.Log.Inner);
            }
        }

        public class Entry
        {
            public DateTime Time;
            public string Text;
            public Entry(
                DateTime time, string text)
            {
                this.Time = time;
                this.Text = text;
            }
            public override string ToString()
            {
                return this.Time.ToString("HH:mm:ss") + ": " + this.Text;
            }
        }
    }
}
