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

            Button save = new("Save", save_Click, w);
            Button load = new("Load", load_Click, w);
            Button settings = new("Settings", settings_Click, w);
            Button debug = new("Debug", debug_Click, w);
            Button help = new("Help", help_Click, w);
            Button quit = new("Quit to main menu", quit_Click, w);
            Button saveexit = new("Save & exit", saveexit_Click, w);
            Button exit = new("Exit to desktop", exit_Click, w);

            this.PanelButtons.AddControlsVertically(settings, debug, help, quit);
            Client.Controls.Add(this.PanelButtons);
            SizeToControl(this.PanelButtons);
            this.AnchorToScreenCenter();
            Title = "Options";
        }

        void saveexit_Click()
        {
            Net.Client.Instance.Disconnect();
            ScreenManager.Remove();
        }

        void quit_Click()
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

        void help_Click()
        {
            HelpWindow.Instance.Toggle();
        }

        void debug_Click()
        {
            GlobalVars.DebugMode = !GlobalVars.DebugMode;
        }

        void exit_Click()
        {
            var exitbox = new MessageBox("Quit game", "Are you sure you want to exit the game without saving?", exitbox_Yes, () => { });
            exitbox.ShowDialog();
        }

        void exitbox_Yes()
        {
            Game1.Instance.Exit();
        }

        void settings_Click()
        {
            SettingsWindow.Instance.ToggleDialog();
        }

        void load_Click()
        {
            Console.WriteLine("load");
        }

        void save_Click()
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
