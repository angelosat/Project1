using System;

namespace Start_a_Town_.UI
{
    class IngameMenu : Window
    {
        public Panel PanelButtons;

        public IngameMenu()
        {
            this.PanelButtons = new();
            this.PanelButtons.AutoSize = true;
            int w = 150;

            Button save = new("Save", Save_Click, w);
            Button load = new("Load", Load_Click, w);
            Button settings = new("Settings", Settings_Click, w);
            Button debug = new("Debug", Debug_Click, w);
            Button help = new("Help", Help_Click, w);
            Button quit = new("Quit to main menu", Quit_Click, w);
            Button saveexit = new("Save & exit", Saveexit_Click, w);
            Button exit = new("Exit to desktop", Exit_Click, w);

            this.PanelButtons.AddControlsVertically(settings, debug, help, quit);
            Client.Controls.Add(this.PanelButtons);
            SizeToControl(this.PanelButtons);
            this.AnchorToScreenCenter();
            Title = "Options";
        }

        void Saveexit_Click()
        {
            Net.Client.Instance.Disconnect();
            ScreenManager.Remove();
        }

        void Quit_Click()
        {
            new MessageBox("Quit game", "Are you sure you want to quit to main menu?",
                new ContextAction(() => "Yes",
                    () =>
                    {
                        Net.Client.Instance.Disconnect();
                        Net.Server.Stop();
                        ScreenManager.Remove();
                    }),
            new ContextAction(() => "No", () => { })).ShowDialog();
        }

        void Help_Click()
        {
            HelpWindow.Instance.Toggle();
        }

        void Debug_Click()
        {
            GlobalVars.DebugMode = !GlobalVars.DebugMode;
        }

        void Exit_Click()
        {
            var exitbox = new MessageBox("Quit game", "Are you sure you want to exit the game without saving?", Exitbox_Yes, () => { });
            exitbox.ShowDialog();
        }

        void Exitbox_Yes()
        {
            Game1.Instance.Exit();
        }

        void Settings_Click()
        {
            SettingsWindow.Instance.ToggleDialog();
        }

        void Load_Click()
        {
            Console.WriteLine("load");
        }

        void Save_Click()
        {
            Console.WriteLine("save");
        }

        //public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        //{
        //    if (e.Handled)
        //        return;
        //    if(e.KeyCode == System.Windows.Forms.Keys.Escape)
        //    {
        //        e.Handled = true;
        //        this.Hide();
        //    }
        //    base.HandleKeyDown(e);
        //}
    }
}
