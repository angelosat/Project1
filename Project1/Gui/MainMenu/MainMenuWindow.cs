using Microsoft.Xna.Framework;
using System.Linq;

namespace Start_a_Town_.UI
{
    class MainMenuWindow : Window
    {
        MessageBox quitbox;
        public MainMenuWindow()
        {
            this.AutoSize = true;
            this.Closable = false;
            var panel = new Panel() { AutoSize = true };
            var newgame = new Button("Play", this.Newgame, 100);
            var load = new Button("Load", this.Load, 100);
            var online = new Button("Multiplayer", this.Online, 100);
            var settings = new Button("Settings", this.Settings, 100);
            var quit = new Button("Quit", this.Quit, 100);

            panel.AddControlsVertically(newgame, load, online, settings, quit);

            this.Client.Controls.Add(panel);
            this.AnchorToScreenCenter();
            this.Title = "Start-a-Town!";
        }

        void Online()
        {
            if (GameMode.Registry.Count == 1)
            {
                GameMode.Current = GameMode.Registry.First();
                MultiplayerWindow.Instance.Show();
            }
        }

        void Settings()
        {
            SettingsWindow.Instance.Show();
        }

        void Quit()
        {
            this.quitbox = MessageBox.Create("Quit game", "Are you sure you want to quit?", Game1.Instance.Exit);
            this.quitbox.ShowDialog();
        }

        void Newgame()
        {
            GameMode.Current = GameMode.Registry.First();
            //this.Hide();

            var client = new GroupBox();
            client.AddControlsVertically(
                    GameMode.Current.GetNewGameGui(() => { this.Show(); client.GetWindow().Hide(); }));

            var win = new Window("New Game", client)
            {
                Movable = false,
                Closable = true
            }
            .AnchorToScreenCenter().Show();
        }

        private void Load()
        {
            if (GameMode.Registry.Count == 1)
                GameMode.Current = GameMode.Registry.First();
            var control = GameMode.Current.LoadGame().ToWindow("Load");
            control.LocationFunc = () => UIManager.Center;
            control.Movable = false;
            control.Anchor = Vector2.One * .5f;
            control.Show();
            //this.Hide();
        }
    }
}
