using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public class Hud : GroupBox
    {
        static Hud()
        {
            HotkeyManager.RegisterHotkey(Ingame.HotkeyContext, "Open chat", delegate { Ingame.Instance.Hud.Chat.StartOrFinishTyping(); }, System.Windows.Forms.Keys.Enter);
            HotkeyManager.RegisterHotkey(ToolManager.HotkeyContextDebug, "Open console", delegate { ServerConsole.Instance.Toggle(); }, System.Windows.Forms.Keys.Oemtilde);
            HotkeyManager.RegisterHotkey(ToolManager.HotkeyContextDebug, "Open debug console", delegate { DebugConsole.Toggle(); }, System.Windows.Forms.Keys.Oem5);
            HotkeyManager.RegisterHotkey(ToolManager.HotkeyContextDebug, "Spawn objects", delegate { UI.Editor.ObjectTemplatesWindow.Instance.ToggleSmart(); }, System.Windows.Forms.Keys.O);
        }
        public new void Initialize()
        {
            foreach (var item in Game1.Instance.GameComponents)
                item.InitHUD(this);
        }

        readonly Dictionary<Message.Types, Action<GameEvent>> UIEvents = new();
        internal void RegisterEventHandler(Message.Types type, Action<GameEvent> action)
        {
            this.UIEvents.Add(type, action);
        }
        public static int DefaultHeight = UIManager.DefaultIconButtonSprite.Height;
        Control WindowPlayers;
        readonly UINpcFrameContainer UnitFrames;
        public Panel PartyFrame;
        public UnitFrame PlayerUnitFrame;
        public Panel Box_Buttons;
        public UIChat Chat;
        public Label Time;
        readonly IngameMenu IngameMenu;
        readonly ScrollbarVNew ZLevelDrawBar;
        readonly IconButton BtnPlayers;
        public void AddButton(IconButton btn)
        {
            btn.Location = this.Box_Buttons.Controls.TopRight;
            this.Box_Buttons.Controls.Add(btn);
            //this.Box_Buttons.Location = UIManager.Size;
            //this.Box_Buttons.Anchor = Vector2.One;
        }

        public Hud(INetwork net, Camera camera)
        {
            this.AutoSize = false;
            this.Width = UIManager.Width;
            this.Height = UIManager.Height;
            this.SetMousethrough(true);

            IconButton BTN_Options = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 0, 32),
                HoverFunc = () => "Menu [" + GlobalVars.KeyBindings.Menu + "]",
                LeftClickAction = BTN_Options_Click
            };
            this.BtnPlayers = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 0, 32),
                HoverFunc = () => "Player list",
                LeftClickAction = () => this.TogglePlayerList(net)
            };

            this.UnitFrames = new UINpcFrameContainer() { LocationFunc = () => new Vector2(UIManager.Width / 2, 0), Anchor = Vector2.UnitX * .5f };
            this.PartyFrame = new Panel();

            this.Box_Buttons = new Panel() { AutoSize = true };//, Location = UIManager.Size };//, Color = Color.Black };
            this.Box_Buttons.AddControlsHorizontally(
                this.BtnPlayers,
                BTN_Options
                );
            //this.Box_Buttons.Anchor = Vector2.One;
            this.Box_Buttons.AnchorToBottomRight();
            this.Box_Buttons.SetMousethrough(true, false);
            this.Controls.Add(this.Box_Buttons);

            var camWidget = new CameraWidget(camera);
            camWidget.AnchorToTopRight();

            var uiSpeed = new UIGameSpeed(net)
            {
                LocationFunc = () => this.Box_Buttons.TopRight,
                Anchor = Vector2.One
            };

            this.Time = new Label()
            {
                LocationFunc = () => uiSpeed.TopRight,
                Anchor = Vector2.One,
                BackgroundColorFunc = () => Color.Black * .5f,
                //TextFunc = () => string.Format("Day {0}, {1:%h}h", (int)net.Map.World.Clock.TotalDays, net.Map.World.Clock)
                TextFunc = () => $"Day {(int)net.Map.World.Clock.TotalDays}, {net.Map.World.Clock:%h}h"
            };

            this.Chat = UIChat.Instance;
            this.Chat.AnchorToBottomLeft();
            this.IngameMenu = new IngameMenu();
            GameMode.Current.OnIngameMenuCreated(this.IngameMenu);

            this.ZLevelDrawBar = new ScrollbarVNew(MapBase.MaxHeight, MapBase.MaxHeight, 1, 16, 1,
                 () => MapBase.MaxHeight - Ingame.GetMap().Camera.DrawLevel,
                 () => 1 / (float)MapBase.MaxHeight,
                 () => Ingame.GetMap().Camera.DrawLevel / MapBase.MaxHeight,
                 v => Ingame.GetMap().Camera.DrawLevel = MapBase.MaxHeight - v);

            //this.ZLevelDrawBar.Location = this.ZLevelDrawBar.RightCenterScreen;
            this.ZLevelDrawBar.AnchorToCenterRight();
            
            this.Controls.Add(
                this.ZLevelDrawBar,
                camWidget, uiSpeed,
                this.Chat
                , this.Time
                , this.UnitFrames
                );
        }

        private void TogglePlayerList(INetwork net)
        {
            if (this.WindowPlayers is null)
            {
                this.WindowPlayers = new UIPlayerList(net.GetPlayers())
                    .ToWidget("Players");
                this.WindowPlayers.Layer = UIManager.LayerHud;
            }
            this.WindowPlayers.Toggle();
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.NotEnoughSpace:
                    this.NotEnoughSpace(e.Parameters[0] as GameObject);
                    break;

                case Message.Types.ItemGot:
                    this.OnItemGot(e.Parameters[0] as GameObject, e.Parameters[1] as GameObject);
                    break;

                case Message.Types.ItemLost:
                    this.OnItemLost(e.Parameters[0] as GameObject, e.Parameters[1] as GameObject, (int)e.Parameters[2]);
                    break;

                case Message.Types.HealthLost:
                    int dmg = (int)e.Parameters[1];
                    GameObject recipient = (GameObject)e.Parameters[0];
                    FloatingText floating = new FloatingText(recipient, dmg.ToString()) { Font = UIManager.FontBold, TextColorFunc = () => Color.Red };
                    floating.Show();
                    break;

                case Message.Types.ChatPlayer:
                    var player = e.Parameters[0] as PlayerData;
                    var txt = (string)e.Parameters[1];
                    Log.Chat(player, txt);
                    break;

                default:
                    if (this.UIEvents.TryGetValue(e.Type, out var val))
                        val(e);
                    base.OnGameEvent(e);
                    break;
            }
        }

        private void NotEnoughSpace(GameObject parent)
        {
            var txt = "Not enough space";
            FloatingTextEx floating = new FloatingTextEx(parent,
                new FloatingTextEx.Segment(txt, Color.White)
                );
            Client.Instance.ConsoleBox.Write(txt);
            floating.Show();
        }

        private void OnItemGot(GameObject parent, GameObject item)
        {
            FloatingTextEx floating = new FloatingTextEx(parent,
                new FloatingTextEx.Segment("Received ", Color.Lime),
                new FloatingTextEx.Segment(item.Name, item.GetInfo().GetQualityColor())
                );
            Client.Instance.ConsoleBox.Write("Received: " + item.Name);
            floating.Show();
        }
        private void OnItemLost(GameObject parent, GameObject item, int amount)
        {
            FloatingTextEx floating = new FloatingTextEx(parent,
                new FloatingTextEx.Segment("Lost " + amount.ToString() + "x ", Color.Red),
                new FloatingTextEx.Segment(item.Name, item.GetInfo().GetQualityColor())
                );
            Client.Instance.ConsoleBox.Write("Lost " + amount.ToString() + "x " + item.Name);
            floating.Show();
        }

        public void Initialize(GameObject obj)
        {
            this.Controls.Remove(this.PartyFrame);
            this.PartyFrame.Controls.Clear();
            this.PartyFrame.AutoSize = true;

            this.PlayerUnitFrame = new UnitFrame().Track(obj);
            this.PartyFrame.Controls.Add(this.PlayerUnitFrame);

            this.Controls.Add(this.PartyFrame);
            this.PartyFrame.Invalidate(true);
        }
        public void Initialize(MapBase map)
        {

        }
        public void AddUnitFrame(GameObject obj)
        {
            this.PartyFrame.Controls.Add(new UnitFrame() { Location = this.PartyFrame.Controls.Last().BottomLeft }.Track(obj));
        }
        public void RemoveUnitFrame(GameObject obj)
        {
            this.PartyFrame.Controls.Remove(this.PartyFrame.Controls.Find(frame => frame.Tag == obj));
        }

        void BTN_Options_Click()
        {
            this.IngameMenu.ShowDialog();
        }
        public override void Reposition(Vector2 ratio)
        {
            foreach (Control ctrl in this.Controls)
                ctrl.Reposition(ratio);
        }

        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;

            if (e.KeyCode == System.Windows.Forms.Keys.Escape)
            {
                e.Handled = true;
                //if (!ToolManager.Clear() && !SelectionManager.ClearTargets() && !this.WindowManager.CloseAll())
                    this.IngameMenu.ToggleDialog();
            }
            HotkeyManager.PerformHotkey(e, Ingame.HotkeyContext);
          
            base.HandleKeyDown(e);
        }
        public override void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.Handled)
                return;
            switch (e.KeyChar)
            {
                case '/':
                    if (this.Chat.TextBox.Enabled)
                    {
                        this.Chat.TextBox.Text += "/";
                        return;
                    }
                    this.Chat.TextBox.Text = "/";
                    this.Chat.StartTyping();
                    break;

                default:
                    base.HandleKeyPress(e);
                    break;
            }
        }
        internal override void OnResolutionChanged()
        {
            this.Width = UIManager.Width;
            this.Height = UIManager.Height;
        }
        public override void OnUIScaleChanged()
        {
            this.Width = UIManager.Width;
            this.Height = UIManager.Height;
        }
    }
}
