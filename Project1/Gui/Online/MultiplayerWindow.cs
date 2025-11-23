using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Start_a_Town_
{
    class MultiplayerWindow : Window
    {
        const int HistoryMax = 16;
        const string DefaultName = "";
        const string DefaultHost = "127.0.0.1";
        public static MultiplayerWindow Instance;
        readonly XElement XRoot, XHistory;
        readonly ObservableCollection<string> History = new();

        static MultiplayerWindow()
        {
            Instance = new MultiplayerWindow();
        }

        readonly Panel PanelTextBoxes, PanelButtons;
        readonly TextBox Txt_Name, TxtBox_HostName;

        MultiplayerWindow()
        {
            this.XRoot = Engine.Config.Root.GetOrCreateElement("Mutiplayer");
            this.XHistory = this.XRoot.GetOrCreateElement("Servers");
            this.LoadServerHistory();

            this.Title = "Multiplayer";
            this.AutoSize = true;
            int btnWidth = 200;
            this.AutoSize = true;
            string hover() => !this.CheckName() ? "Invalid name" : "";
            var joinButton = new Button("Join game", connect, btnWidth)
            {
                HoverFunc = hover
            };

            var name = DefaultName;
            var namenode = Engine.Config.Root.Descendants("PlayerName").FirstOrDefault();
            name = namenode != null ? namenode.Value : name;
            var host = this.History.FirstOrDefault() ?? DefaultHost;
            var labelname = new Label("Player name");
            this.Txt_Name = new TextBox(name, btnWidth) { Location = this.Client.Controls.TopRight };

            var labelhost = new Label("Host address");
            this.TxtBox_HostName = new TextBox(host, btnWidth) { InputFilter = c => char.IsLetterOrDigit(c) || char.IsPunctuation(c) };

            this.PanelTextBoxes = new Panel() { AutoSize = true };
            this.PanelTextBoxes.AddControlsVertically(
                labelname,
                this.Txt_Name,
                labelhost,
                this.TxtBox_HostName
                );

            this.PanelButtons = new Panel() { AutoSize = true };
            this.PanelButtons.AddControls(joinButton);

            var guiHistory = new ScrollableBoxNewNew(btnWidth, 100, ScrollModes.Vertical);
            var historyList = new TableObservable<string>()
                .AddColumn("name", guiHistory.Client.Width - Icon.Cross.Width, a => new Label(a, () => this.TxtBox_HostName.Text = a))
                .AddColumn("delete", Icon.Cross.Width, a => IconButton.CreateSmall(Icon.Cross, () => removeFromHistory(a)).ShowOnParentFocus(true))
                .Bind(this.History);
            
            guiHistory.AddControls(historyList);

            this.Client.AddControlsVertically(this.PanelTextBoxes,
                guiHistory.ToPanelLabeled("Server History"),
                this.PanelButtons
                );

            void removeFromHistory(string a)
            {
                this.History.Remove(a);
                this.XHistory.Elements().First(x => x.Value == a).Remove();
                Engine.SaveConfig();
            }
            void connect()
            {
                if (this.CheckName())
                    this.ConnectTo(this.TxtBox_HostName.Text);
            }
        }

        private void LoadServerHistory()
        {
            foreach (var i in this.XHistory.Elements())
                this.History.Add(i.Value);
        }

        private void Dedicated()
        {
            Server.Start();
            ServerUI.Instance.Show();

        }

        private bool CheckName()
        {
            return !(string.IsNullOrEmpty(this.Txt_Name.Text) || string.IsNullOrWhiteSpace(this.Txt_Name.Text));
        }

        private void Host()
        {
            Server.Start();
            string localHost = "127.0.0.1";
            Net.Client.Instance.Connect(localHost, new PlayerData(this.Txt_Name.Text), a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
            LobbyWindow.Instance.Show();
        }

        public override bool Show()
        {
            this.SnapToScreenCenter();
            return base.Show();
        }

        private void ConnectTo(string address)
        {
            this.SaveFieldsToConfig();
            UIConnecting.Create(address);
            Engine.PlayGame();
            Task.Factory.StartNew(() =>
                Net.Client.Instance.Connect(address, new PlayerData(this.Txt_Name.Text), ar =>
                {
                    LobbyWindow.Instance.Console.Write("Connected to " + address);
                }));
        }
        private void SaveFieldsToConfig()
        {
            var name = this.Txt_Name.Text;
            var host = this.TxtBox_HostName.Text;

            this.XRoot.GetOrCreateElement("PlayerName").Value = name;
            this.XRoot.GetOrCreateElement("HostAddress").Value = host;

            if (!this.History.Contains(host))
            {
                this.History.Insert(0, host);// new(host));
                if (this.History.Count > HistoryMax)
                    this.History.RemoveAt(this.History.Count - 1);
                this.XHistory.AddFirst(new XElement("Address", host));
                if (this.XHistory.Descendants() is var children && children.Count() > HistoryMax)
                    children.Last().Remove();
            }
            Engine.SaveConfig();
        }
    }
}
