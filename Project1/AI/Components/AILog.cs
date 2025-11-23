using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Start_a_Town_.Components;
using Start_a_Town_.Modules.AI.Net.Packets;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    public class AILog
    {
        const int Capacity = 64;
        readonly ObservableCollection<Entry> Inner = new();

        static public void SyncWrite(GameObject entity, string text)
        {
            var net = entity.Net;
            if (net is not Server)
                return;
            AIState state = AIState.GetState(entity);
            PacketAILogWrite.Send(net as Server, entity.RefID, text);
            state.History.Write(text);
        }
        static public bool TryWrite(GameObject entity, string text)
        {
            if (!entity.HasComponent<AIComponent>())
                return false;
            var state = AIState.GetState(entity);
            state.History.WriteEntry(entity as Actor, text);
            return true;
        }
        public Entry WriteEntry(GameObject entity, string text)
        {
            var entry = new Entry(DateTime.Now, text);
            this.Inner.Add(entry);
            if (this.Inner.Count > Capacity)
                this.Inner.RemoveAt(0);
            entity.Net.Map.EventOccured(Message.Types.AILogUpdated, entity, this, entry);
            return entry;
        }
        public Entry Write(string text)
        {
            var entry = new Entry(DateTime.Now, text);
            this.Inner.Add(entry);
            if (this.Inner.Count > Capacity)
                this.Inner.RemoveAt(0);
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
                    .AddColumn(null, "Description", 400, (e) => new GroupBox().AddControlsLineWrap(Label.ParseNew(e.Text)), 0));

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
