using Project1.Core;
using Project1.Core.Base;
using Project1.Core.Helpers;
using Project1.Core.UI;
using System.Xml.Linq;

namespace Project1.Core.UI.Settings
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
