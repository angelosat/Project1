using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Entities;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.UI;
using Project1.Core.UI.Hud;
using Project1.Core.UI.NamePlates;
using Project1.Framework.Events;
using Project1.Framework.Input;
using Project1.Framework.UI;

namespace Project1.Core.Screens;

internal class Ingame : GameScreen
{
    static Ingame _instance;
    static public Ingame Instance => _instance ??= new();
        
    public NotificationArea NotificationArea;
    public NameplateManager NameplateManager;// = new();
    public static readonly HotkeyCategory HotkeyContextInterface = new("Ingame");
    public EventBus Events = new();

    bool HideInterface = false;
    public SceneState Scene = new();
    //public override Camera Camera => MainViewportMap.Camera; 

    public override GameScreen Initialize(NetEndpoint net)
    {
        //var map = net.Map.Camera;
        var map = net.MainViewport.Map;
        var camera = net.MainViewport.Camera;
        if (net is Server)
            DrawServer = true;
        WindowManager = new UIManager();
        NotificationArea = new NotificationArea();
        this.Hud = new Hud(net, camera);
        this.Hud.Initialize(net);
        GameMode.Current.OnHudCreated(this.Hud);
        net.World.MainMap.World.OnHudCreated(this.Hud);
        this.Hud.Show(WindowManager);
        this.NameplateManager = new NameplateManager(net.World.MainMap);
        this.NameplateManager.Show(WindowManager);
        this.ToolManager = ToolManager.Instance;
        this.ToolManager.Bind(net.World.MainMap);

        this.InputRouter.Add(this.ToolManager);
        this.InputRouter.Add(this.WindowManager);
        this.InputRouter.Add(ContextMenuManager.Instance);
        this.InputRouter.Add(Game1.Renderer);
        this.InputRouter.Add(this);

        SelectionManager.Instance.Bind(net);
        SelectionManager.Instance.Init(this);
        TooltipManager.Bind(net);
        Registry.PlayerInputEventHooks.HookTo(this.Events);
        return this;
    }
    static public NetEndpoint Net => DrawServer ? Server.Instance : Client.Instance;
    //public override Renderer Renderer => Net.MainViewport.Renderer;
    public Hud Hud;
    public override void Update(Game1 game, GameTime gt)
    {
        base.Update(game, gt);
        //var map = DrawServer? Server.Instance.Map : Client.Instance.Map;
        var viewport = MainViewport;
        var map = viewport.Map;
        ToolManager.Update(map, this.Scene);
        viewport.Camera.Update(map);
        WindowManager.Update(game, gt);
        NotificationArea.Update();
    }
    public override void Draw(SpriteBatch sb, Renderer renderer)
    {
        this.Scene.ObjectBounds.Clear();
        this.Scene.ObjectsDrawn.Clear();

        var viewport = MainViewport;
        renderer.DrawMap(viewport, ToolManager, WindowManager, Scene);
        ToolManager.DrawUI(sb, viewport);
        DrawInterface(sb, Scene);
        NotificationArea.Draw(sb);
    }

    private void DrawInterface(SpriteBatch sb, SceneState scene)
    {
        if (HideInterface)
            return;

        //var cam = DrawServer ? Server.Instance.Map.Camera : Client.Instance.Map.Camera;
        WindowManager.Draw(sb, MainViewport);
    }

    internal override void OnGameEvent(GameEvent e)
    {
        this.NameplateManager.OnGameEvent(e);
        base.OnGameEvent(e);
    }

    //static public MapBase GetMap()
    //    => DrawServer ? Server.Instance.MainViewport.Map : Client.Instance.MainViewport.Map;

    static public MapBase MainViewportMap => MainViewport.Map;
    static public Camera MainViewportCamera => MainViewport.Camera;
    static public MapViewport MainViewport => DrawServer ? Server.Instance.MainViewport : Client.Instance.MainViewport;

    static public bool DrawServer;
    public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
    {
        //base.HandleKeyDown(e);
        if (e.Handled)
            return;

        var pressed = InputState.Instance.GetPressedKeys();
        if (pressed.Contains(GlobalVars.KeyBindings.HideInterface))
            HideInterface = !HideInterface;
        if (pressed.Contains(System.Windows.Forms.Keys.F6))
        {
            DrawServer = !DrawServer;
            //GetMap().Camera.TopSliceChanged = true;
            //this.Hud.Chat.Write(Log.EntryTypes.System, string.Format("draw server: {0}", DrawServer));
            Client.Instance.ChatService.Post(ChatSource.System, $"draw server: {DrawServer}");
        }
    }
}
