using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.UI
{
    public enum ConsoleMessageTypes { Acks }

    public class ConsoleBoxAsync : ScrollableBoxNewNew
    {
        public readonly object Lock = new();
        public FilterType FilterType = FilterType.Exclude;
        public HashSet<ConsoleMessageTypes> Filters = new() { ConsoleMessageTypes.Acks };

        public bool Filter(ConsoleMessageTypes i)
        {
            if (this.Filters.Contains(i))
                return this.FilterType == FilterType.Include;
            return this.FilterType == FilterType.Exclude;
        }

        public bool FadeText;
        public bool TimeStamp = true;
        public ConsoleBoxAsync(Rectangle bounds)
            : base(bounds.Width, bounds.Height, ScrollModes.Vertical)
        {
            this.LoadConfig();
            this.BackgroundColor = Color.Black;
            this.Opacity = 0f;
        }

        public ConcurrentBag<string> Entries = new();

        public void Write(string name, string text)
        {
            this.Write(Color.White, name, text);
        }
        public void Write(string text)
        {
            this.Write(Color.White, text);
        }
        public void Write(Color c, string text)
        {
            // TODO i don't want to write directly here. write to the logwindow that has this consoleboxasync bound
            this.Entries.Add(text);
#if DEBUG
            text.ToConsole();
#endif
            lock (this.Lock)
            {
                if (this.TimeStamp)
                    text = text.Insert(0, DateTime.Now.ToString("[HH:mm:ss]"));
                text = StringHelper.Wrap(text, this.Client.Width);

                if (this.Client.Controls.Count >= 16)
                    this.Client.RemoveControls(this.Client.Controls.First());

                //var line = new Label(text) { TextColorFunc = () => c };
                var line = new ConsoleEntry(c, text);
                line.BackgroundColor = Color.Black * .5f;

                this.AddControls(line); //add line after removing oldest one so that the box height doesn't increase?
                this.AlignTopToBottom();

                this.Client.ClientLocation.Y = this.Client.Bottom - this.Client.ClientSize.Height;
            }
        }
       
        public void Write(ConsoleEntry line)
        {
            this.Entries.Add(line.Text);
            this.AddControls(line); //add line after removing oldest one so that the box height doesn't increase?
            this.AlignTopToBottom();
            this.Client.ClientLocation.Y = this.Client.Bottom - this.Client.ClientSize.Height;
        }
        public void Write(Log.Entry entry)
        {
            this.Entries.Add(entry.ToString());
            this.AddControls(entry.GetGuiNew(this.Client.Width)); //add line after removing oldest one so that the box height doesn't increase?
            this.AlignTopToBottom();
            this.Client.ClientLocation.Y = this.Client.Bottom - this.Client.ClientSize.Height;
        }
        public void Write(ConsoleMessageTypes type, Color c, string name, string text)
        {
            this.Entries.Add(text);
            if (!this.Filter(type))
                return;
            lock (this.Lock)
            {
                if (this.TimeStamp)
                    text = text.Insert(0, DateTime.Now.ToString("[HH:mm:ss]" + "[" + name + "]: "));
                text = StringHelper.Wrap(text, this.Client.Width);
                var line = new Label(this.Client.Controls.BottomLeft, text) { TextColorFunc = () => c };

                this.AddControls(line);
                this.Client.ClientLocation.Y = this.Client.Bottom - this.Client.ClientSize.Height;
            }
        }
        public void Write(Color c, string name, string text)
        {
            this.Entries.Add(text);
            lock (this.Lock)
            {
                text = text.Insert(0, DateTime.Now.ToString("[" + name + "]: "));
                this.Write(c, text);
            }
        }
        public override Control AddControls(params Control[] controls)
        {
            foreach (var c in controls)
                c.Tag = LogWindow.FadeDelay;
            return base.AddControls(controls);
        }
        private void LoadConfig()
        {
            Engine.Config.Root.TryGetValue("Timestamps", v =>
            {
                if (bool.TryParse(v, out bool parsed))
                    this.TimeStamp = parsed;
            });
        }
    }
}
