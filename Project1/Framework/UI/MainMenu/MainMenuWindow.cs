using Microsoft.Xna.Framework;
using Project1.Framework;
using Project1.Framework.Base;
using Project1.Framework.UI;
using System.Linq;
using System.Windows.Forms;
using Button = Project1.Framework.UI.Button;
using GroupBox = Project1.Framework.UI.GroupBox;

namespace Start_a_Town_.UI
{
    class MainMenuWindow : Window
    {
        MessageBox quitbox;
        public MainMenuWindow()
        {
            this.AutoSize = true;
            this.Closable = false;
            var newgame = new Button("Play", this.Newgame, 100);
            var load = new Button("Load", this.Load, 100);
            var online = new Button("Multiplayer", this.Online, 100);
            var settings = new Button("Settings", this.Settings, 100);
            var quit = new Button("Quit", this.Quit, 100);

            this.Client.AddControlsVertically(newgame, load, online, settings, quit);

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
