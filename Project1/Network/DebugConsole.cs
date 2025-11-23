using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Start_a_Town_
{
    internal class DebugConsole
    {
        internal class ConsoleEntry
        {
            readonly public string Name;
            readonly public Color Color;

            public ConsoleEntry(string name, Color color)
            {
                Name = name;
                Color = color;
            }
        }

        static readonly int EntryCap = 128;

        static public readonly ConsoleEntry System = new("System", Color.Yellow);
        static public readonly ConsoleEntry Debug = new("Debug", Color.Yellow);
        static public readonly ConsoleEntry Error = new("Error", Color.Red);
        static public readonly ConsoleEntry Warning = new("Warning", Color.Orange);

        static readonly ObservableCollection<(DateTime, ConsoleEntry, string)> Entries = new();
        static readonly GuiWorker Gui = new(Entries);

        static public void Write(string text)
        {
            Write(null, text);
        }
        static public void Write(ConsoleEntry entryType, string text)
        {
            Entries.Add((DateTime.Now, entryType, text));
        }
        static public void Toggle()
        {
            Gui.Container.BackgroundColor = Color.Black * .5f;
            Gui.Container.Toggle();
        }
        class GuiWorker
        {
            GroupBox Console;
            public ScrollableBoxTest Container;

            static int Width => UIManager.Width;
            public GuiWorker(ObservableCollection<(DateTime, ConsoleEntry, string)> entries)
            {
                this.Console = new GroupBox();
                this.Container = new(this.Console, UIManager.Width, UIManager.Height);
                entries.CollectionChanged += Entries_CollectionChanged;
            }

            private void Entries_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                foreach (var item in e.NewItems.Cast<(DateTime, ConsoleEntry, string)>())
                    this.Console.AddControlsBottomLeft(Parse(item));

                if (this.Console.Controls.Count > EntryCap)
                {
                    var amountToRemove = this.Console.Controls.Count - EntryCap;
                    this.Console.RemoveControls(this.Console.Controls.Take(amountToRemove));
                    this.Console.Controls.AlignVertically();
                }
            }

            static Control Parse((DateTime, ConsoleEntry, string) entry)
            {
                var line = new GroupBox();
                //var typeLabel = new Label(entry.Item1.Name) { TextColor = entry.Item1.Color };
                //line.AddControlsHorizontally(typeLabel, Label.ParseWrap(Width - typeLabel.Width, entry.Item2));
                var col = entry.Item2?.Color ?? UIManager.DefaultTextColor;
                line.AddControls(new Label($"[{entry.Item1.ToString("HH:MM:ss")}] ") { TextColor = col });
                int xOffset = line.Width;
                if (entry.Item2 != null)
                {
                    line.AddControlsTopRight(new Label($"{entry.Item2.Name} ") { TextColor = col });
                    xOffset += line.Width;
                }
                line.AddControlsTopRight(Label.ParseWrap(Width - xOffset, entry.Item3));
                return line;
            }
        }
    }
}
