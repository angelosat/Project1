using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Framework.UI;
using Project1.Core.UI.Hud.Chat;

namespace Project1.Core
{
    public partial class Log
    {
        public class Entry
        {
            bool ShowSource;// = true;
            public Color Color = Color.White;
            public object Source;
            public EntryTypes Type;
            public object[] Values;
            public Entry(EntryTypes type, object[] values)
            {
                this.Type = type;
                this.Source = type;
                this.Values = values;
            }
            public Entry(EntryTypes type, object source, object[] values)
            {
                this.Type = type;
                this.Source = source;
                this.Values = values;
            }
            
            public ConsoleEntryNew GetGuiNew(int maxWidth)
            {
                var box = new ConsoleEntryNew();
                var controls = LabelNew.ParseNewNew(this.Values);
                if (this.ShowSource)
                    controls = controls.Prepend(new LabelNew($"[{this.Source}]") { TextColor = this.Color, Font = UIManager.FontBold });
                box.AddControlsLineWrap(controls, maxWidth);
                return box;
            }
            public override string ToString()
            {
                return this.ConvertToString();
            }
            string ConvertToString()
            {
                return $"[{this.Source}] {string.Join(" ", this.Values.Select(v => v is string ? v.ToString() : $"[{v}]"))}";
            }
          
            public static Entry Notification(params object[] values)
            {
                return new Entry(EntryTypes.Notification, values) { Color = Color.Goldenrod };
            }
            public static Entry Warning(string text)
            {
                return new Entry(EntryTypes.Warning, [text]) { Color = Color.Orange };
            }
            public static Entry Error(string text)
            {
                return new Entry(EntryTypes.Error, [text]) { Color = Color.Red };
            }
            public static Entry System(string text)
            {
                return new Entry(EntryTypes.System, [text]) { Color = Color.Yellow };
            }
            
            public static Entry Network(object source, string text)
            {
                return new Entry(EntryTypes.Network, source, [text]) { Color = Color.Lime };
            }
        }
    }
}
