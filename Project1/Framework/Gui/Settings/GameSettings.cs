using Start_a_Town_.UI;
using System.Xml.Linq;

namespace Start_a_Town_
{
    abstract class GameSettings
    {
        internal abstract GroupBox Gui { get; }
        internal abstract void Apply();
        internal abstract void Cancel();
        internal virtual void Defaults() { }
        internal abstract string Name { get; }

        static XElement _xmlNodeSettings;
        public static XElement XmlNodeSettings => _xmlNodeSettings ??= Engine.XmlNodeSettings.GetOrCreateElement("Settings");

        internal static void Init()
        {
            HotkeyManager.Import();
        }
    }
}
