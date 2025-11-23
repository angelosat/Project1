using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    partial class HotkeyManager : GameSettings
    {
        static readonly HashSet<Hotkey> Hotkeys = new();
        GroupBox _gui;
        internal override GroupBox Gui => this._gui ??= this.CreateGui();
        readonly Lazy<WindowInputKey> EnterKeyGui = new();
        internal override string Name => "Hotkeys";

        public static IHotkey RegisterHotkey(HotkeyContext context, string label, Action action, System.Windows.Forms.Keys key1 = System.Windows.Forms.Keys.None, System.Windows.Forms.Keys key2 = System.Windows.Forms.Keys.None)
        {
            return RegisterHotkey(context, label, action, delegate { }, key1, key2);
        }
        public static IHotkey RegisterHotkey(HotkeyContext context, string label, Action actionPress, Action actionRelease, System.Windows.Forms.Keys key1 = System.Windows.Forms.Keys.None, System.Windows.Forms.Keys key2 = System.Windows.Forms.Keys.None)
        {
            var hotkey = new Hotkey(context, label, actionPress, actionRelease, key1, key2);
            HandleConflicts(hotkey, false);
            Hotkeys.Add(hotkey);
            return hotkey;
        }
        static void HandleConflicts(Hotkey newHotkey, bool overwrite)
        {
            if (newHotkey.Key1 == System.Windows.Forms.Keys.None && newHotkey.Key2 == System.Windows.Forms.Keys.None)
                return;
            foreach (var h in Hotkeys.Where(hk => hk.Context == newHotkey.Context))
            {
                if (h == newHotkey)
                    continue;
                if (newHotkey.Key1 != 0)
                {
                    if (h.ShortcutKeys[0] == newHotkey.Key1)
                    {
                        if (!overwrite)
                            newHotkey.Key1 = 0;
                        else
                            h.Key1 = 0;
                        return;
                    }
                    else if (h.ShortcutKeys[1] == newHotkey.Key1)
                    {
                        if (!overwrite)
                            newHotkey.Key1 = 0;
                        else
                            h.Key2 = 0;
                        return;
                    }
                }
                if (newHotkey.Key2 != 0)
                {
                    if (h.ShortcutKeys[0] == newHotkey.Key2)
                    {
                        if (!overwrite)
                            newHotkey.Key2 = 0;
                        else
                            h.Key1 = 0;
                        return;
                    }
                    else if (h.ShortcutKeys[1] == newHotkey.Key2)
                    {
                        if (!overwrite)
                            newHotkey.Key2 = 0;
                        else
                            h.Key2 = 0;
                        return;
                    }
                }
            }
        }

        public static bool PerformHotkey(System.Windows.Forms.Keys key, HotkeyContext context)
        {
            if (Hotkeys.FirstOrDefault(h => h.Context == context && h.ShortcutKeys.Contains(key)) is not Hotkey hotkey)
                return false;
            hotkey.ActionPress();
            return true;
        }
        /// <summary>
        /// Sets KeyEventArgs.Handled to true if hotkey was performed
        /// </summary>
        /// <param name="e"></param>
        /// <param name="context"></param>
        public static void PerformHotkey(System.Windows.Forms.KeyEventArgs e, HotkeyContext context)
        {
            if (Hotkeys.FirstOrDefault(h => h.Context == context && h.ShortcutKeys.Contains(e.KeyCode)) is not Hotkey hotkey)
                return;
            hotkey.ActionPress();
            e.Handled = true;
        }
        public static bool Press(System.Windows.Forms.Keys key, HotkeyContext context)
        {
            if (Hotkeys.FirstOrDefault(h => h.Context == context && h.ShortcutKeys.Contains(key)) is Hotkey hotkey && !hotkey.Pressed)
            {
                hotkey.Pressed = true;
                hotkey.ActionPress();
                return true;
            }
            return false;
        }
        public static bool Release(System.Windows.Forms.Keys key, HotkeyContext context)
        {
            if (Hotkeys.FirstOrDefault(h => h.Context == context && h.ShortcutKeys.Contains(key)) is Hotkey hotkey && hotkey.Pressed)
            {
                hotkey.Pressed = false;
                hotkey.ActionRelease();
                return true;
            }
            return false;
        }
        internal override void Apply()
        {
            foreach (var hk in Hotkeys)
                hk.Apply();
            Export();
        }
        internal override void Cancel()
        {
            foreach (var hk in Hotkeys)
                hk.Undo();
        }
        GroupBox CreateGui()
        {
            var box = new GroupBox() { Name = "Hotkeys" };
            var byContext = Hotkeys.GroupBy(h => h.Context);
            box.AddControlsVertically(1, byContext.Select(cat =>
                new TableScrollableCompact<Hotkey>(true)
                    .AddColumn(null, cat.Key.Name, 192, h => h.Label.ToLabel())
                    .AddColumn(null, "Primary", 64, h => new Label(() => $"{h.Key1}", delegate { editHotkey(h, 0); }))
                    .AddColumn(null, "Secondary", 64, h => new Label(() => $"{h.Key2}", delegate { editHotkey(h, 1); }))
                    .AddItems(cat)).ToArray());
            box.Validate(true);
            return box;
            static void setHotkey(Hotkey hotkey, int keyIndex, System.Windows.Forms.Keys key)
            {
                hotkey._keys[keyIndex] = key;
                HandleConflicts(hotkey, true);
            }

            void editHotkey(Hotkey h, int index)
            {
                this.EnterKeyGui.Value.EditHotkey(h.Label, key => setHotkey(h, index, key));
            }
        }

        static void Export()
        {
            var config = Engine.Config;
            var byContext = Hotkeys.GroupBy(h => h.Context);
            var rootNode = XmlNodeSettings.GetOrCreateElement("Hotkeys");
            foreach(var context in byContext)
            {
                var contextNode = rootNode.GetOrCreateElement(context.Key.Name);
                var childrenByLabel = contextNode.Elements().ToDictionary(x => x.Attribute("Label").Value, x => x);
                foreach (var key in context)
                {
                    if(!childrenByLabel.TryGetValue(key.Label, out var keynode))
                    {
                        keynode = new System.Xml.Linq.XElement("Hotkey");
                        contextNode.Add(keynode);
                    }
                    keynode.SetAttributeValue("Label", key.Label);
                    key.XWrite(keynode);
                }
            }
        }
        public static void Import()
        {
            var config = Engine.Config;
            var rootNode = XmlNodeSettings.GetOrCreateElement("Hotkeys");
            var hotkeyNodes = rootNode.Descendants("Hotkey");
            var byLabel = Hotkeys.ToDictionary(h => h.Label, h => h);
            foreach(var hkn in hotkeyNodes)
            {
                var label = hkn.Attribute("Label").Value;
                if (byLabel.TryGetValue(label, out var hotkey))
                    hotkey.XRead(hkn);
            }
        }
    }
}
