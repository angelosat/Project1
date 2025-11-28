using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class Ingame : GameScreen
    {
        static Ingame _instance;
        static public Ingame Instance => _instance ??= new();
            
        public NotificationArea NotificationArea;
        public NameplateManager NameplateManager = new();
        public static readonly HotkeyContext HotkeyContext = new("Ingame");

        bool HideInterface = false;
        public SceneState Scene = new();
        public override Camera Camera => GetMap().Camera; 

        public override GameScreen Initialize(INetPeer net)
        {
            var camera = net.Map.Camera;
            if (net is Net.Server)
                DrawServer = true;
            WindowManager = new UIManager();
            NotificationArea = new NotificationArea();
            this.Hud = new Hud(net, camera);
            this.Hud.Initialize();
            this.Hud.Initialize(net.Map);
            GameMode.Current.OnHudCreated(this.Hud);
            net.Map.World.OnHudCreated(this.Hud);
            this.Hud.Show(WindowManager);
            this.NameplateManager.Show(WindowManager);
            ToolManager = ToolManager.Instance;
            KeyHandlers.Clear();
            KeyHandlers.Push(ToolManager);
            KeyHandlers.Push(WindowManager);
            KeyHandlers.Push(ContextMenuManager.Instance);
            KeyHandlers.Push(this.Camera);
            return this;
        }

        static public INetPeer Net => DrawServer ? Server.Instance : Client.Instance;
        public Hud Hud;
        public override void Update(Game1 game, GameTime gt)
        {
            base.Update(game, gt);
            var map = DrawServer? Server.Instance.Map : Client.Instance.Map;
            ToolManager.Update(map, this.Scene);
            map.Camera.Update(map);
            WindowManager.Update(game, gt);
            this.NameplateManager.Update(this.Scene);
            NotificationArea.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            this.Scene.ObjectBounds.Clear();
            this.Scene.ObjectsDrawn = new HashSet<GameObject>();

            var map = GetMap();
            map.Draw(ToolManager, WindowManager, Scene);

            /// i moved this to camera.newdraw method
            /// DONT ERASE
            //sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);
            //map.DrawInterface(sb, map.Camera);
            //sb.End();
            /// DONT ERASE
            /// 

            ToolManager.DrawUI(sb, map);
            //NameplateManager.Draw(sb);
            DrawInterface(sb, Scene);
            NotificationArea.Draw(sb);
            this.NameplateManager.Update(this.Scene);
        }

        private void DrawInterface(SpriteBatch sb, SceneState scene)
        {
            if (HideInterface)
                return;

            var cam = DrawServer ? Server.Instance.Map.Camera : Client.Instance.Map.Camera;
            WindowManager.Draw(sb, cam);
        }

        internal override void OnGameEvent(GameEvent e)
        {
            this.NameplateManager.OnGameEvent(e);
            base.OnGameEvent(e);
        } 
 
        static public MapBase GetMap()
        {
            return DrawServer ? Server.Instance.Map : Client.Instance.Map;
        }
       
        static public MapBase CurrentMap => DrawServer ? Server.Instance.Map : Client.Instance.Map;
        static public bool DrawServer;
        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            base.HandleKeyDown(e);
            if (e.Handled)
                return;

            List<System.Windows.Forms.Keys> pressed = Controller.Input.GetPressedKeys();
            if (pressed.Contains(GlobalVars.KeyBindings.HideInterface))
                HideInterface = !HideInterface;
            if (pressed.Contains(System.Windows.Forms.Keys.F6))
            {
                DrawServer = !DrawServer;
                GetMap().Camera.TopSliceChanged = true;
                this.Hud.Chat.Write(Log.EntryTypes.System, string.Format("draw server: {0}", DrawServer));
            }
        }
    }
}
