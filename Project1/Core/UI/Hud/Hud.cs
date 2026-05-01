using Microsoft.Xna.Framework;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Towns.UI;
using Project1.Core.UI.Hud.Chat;
using Project1.Framework;
using Project1.Framework.UI;

namespace Project1.Core.UI.Hud;

[EnsureStaticCtorCall]
public sealed class Hud : GroupBox
{
    static Hud()
    {
        HotkeyManager.RegisterHotkey(Ingame.HotkeyContextInterface, "Open chat", delegate { Ingame.Instance.Hud.Chat.StartOrFinishTyping(); }, System.Windows.Forms.Keys.Enter);
        //HotkeyManager.RegisterHotkey(ToolManager.HotkeyContextDebug, "Open console", delegate { ServerConsole.Instance.Toggle(); }, System.Windows.Forms.Keys.Oemtilde);
        //HotkeyManager.RegisterHotkey(ToolManager.HotkeyContextDebug, "Open debug console", delegate { DebugConsole.Toggle(); }, System.Windows.Forms.Keys.Oem5);
        //HotkeyManager.RegisterHotkey(ToolManager.HotkeyContextDebug, "Spawn objects", delegate { ObjectTemplatesWindow.Instance.ToggleSmart(); }, System.Windows.Forms.Keys.O);
    }
    public void Initialize(NetEndpoint net)
    {
        foreach (var item in Game1.Instance.GameComponents)
            item.InitHUD(net, this);
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
    Control StockpileTracker;
    public void AddButton(IconButton btn)
    {
        btn.Location = this.Box_Buttons.Controls.TopRight;
        this.Box_Buttons.Controls.Add(btn);
    }

    public Hud(NetEndpoint net, ICamera camera)
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

        this.UnitFrames = new UINpcFrameContainer(net.MainViewport.Map) { LocationFunc = () => new Vector2(UIManager.Width / 2, 0), Anchor = Vector2.UnitX * .5f };
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
            TextFunc = () => $"Day {(int)net.World.Clock.TotalDays}, {net.World.Clock:%h}h {net.World.Clock:%m}m"
        };

        this.Chat = new(net);// UIChat.Instance;
        //this.Chat.Write($"Connected to {(net as Client).RemoteIP}");
        net.ChatService.Post(ChatSource.System, $"Connected to {(net as Client).RemoteIP}");
        this.Chat.AnchorToBottomLeft();
        this.IngameMenu = new IngameMenu();
        GameMode.Current.OnIngameMenuCreated(this.IngameMenu);

        this.ZLevelDrawBar = new ScrollbarVNew(MapBase.MaxHeight, MapBase.MaxHeight, 1, 16, 1,
             () => MapBase.MaxHeight - Ingame.MainViewport.Settings.DrawLevel,
             () => 1 / (float)MapBase.MaxHeight,
             () => Ingame.MainViewport.Settings.DrawLevel / MapBase.MaxHeight,
             //v => Ingame.MainViewport.Renderer.DrawLevel = MapBase.MaxHeight - v);
             v => Ingame.MainViewport.SetDrawLevel(MapBase.MaxHeight - v));

        this.ZLevelDrawBar.AnchorToCenterRight();

        //this.StockpileTracker = net.MainViewport.Map.Hauling.Tracker.GetControl();
        this.StockpileTracker = net.MainViewport.Map.Hauling.TrackerManager.GetControl();

        this.Controls.Add(
            this.ZLevelDrawBar,
            camWidget, uiSpeed,
            this.Chat
            , this.Time
            , this.UnitFrames
            //, this.StockpileTracker
            );
        this.Controls.Add(this.StockpileTracker);

        //this.Controls.Add(new LabelNew(() => "TESTpppggg") { BackgroundColorFunc=()=>Color.Red }.AnchorToScreenCenter());
    }

    private void TogglePlayerList(NetEndpoint net)
    {
        if (this.WindowPlayers is null)
        {
            this.WindowPlayers = new UIPlayerList(net)
                .ToWidget("Players");
            this.WindowPlayers.Layer = UIManager.LayerHud;
        }
        this.WindowPlayers.Toggle();
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
                this.IngameMenu.ToggleDialog();
        }
        HotkeyManager.PerformHotkey(e, Ingame.HotkeyContextInterface);
      
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
