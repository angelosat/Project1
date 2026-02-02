using System;
using System.Linq;
using System.Reflection;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class SettingsWindow : Window
    {
        static SettingsWindow _Instance;
        static public SettingsWindow Instance => _Instance ??= new SettingsWindow();

        SettingsWindow()
        {
            this.Title = "Settings";
            this.Closable = false;
            this.AutoSize = true;
            GameSettings selectedSettings = null;

            var size = 400;
            var panel = new PanelLabeledScrollable(() => selectedSettings?.Name, size, size / 2, ScrollModes.Vertical);
            //var panel = new PanelLabeledNew(() => selectedSettings?.Name);//, ScrollModes.Vertical);

            var settingsTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(GameSettings)));
            var allSettings = settingsTypes.Select(t => Activator.CreateInstance(t) as GameSettings).ToList(); // need to consolidate the ienumerable otherwise new instances are created when iterating

            var tabs = UIHelper.Wrap(allSettings.Select(tab => new Button(tab.Gui.Name, () => selectTab(tab))), panel.Client.Width);

            selectTab(allSettings.First());

            var btnok = new Button("Ok", ok, 50);
            var btnapply = new Button("Apply", apply, 50);
            var btncancel = new Button("Cancel", cancel, 50);
            var btndefaults = new Button("Defaults", defaults, 50);

            this.Client.AddControlsVertically(0, Alignment.Horizontal.Right,
                tabs.ToPanel(),
                panel,
                UIHelper.Wrap(btnok, btnapply, btncancel, btndefaults));

            this.AnchorToScreenCenter();
            void ok()
            {
                apply();
                this.Hide();
            }
            void apply()
            {
                foreach (var m in allSettings)
                    m.Apply();
                Engine.SaveConfig();
            }
            void cancel()
            {
                foreach (var m in allSettings)
                    m.Cancel();
                this.Hide();
            }
            void defaults()
            {
                selectedSettings.Defaults();
            }
            void selectTab(GameSettings settings)
            {
                var tab = settings.Gui;
                selectedSettings = settings;
                panel.Client.ClearControls();
                panel.Client.AddControls(tab);
            }
        }
    }
}