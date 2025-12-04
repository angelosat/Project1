using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public class ToolManager : IKeyEventHandler
    {
        static ToolManager _instance;
        public static ToolManager Instance => _instance ??= new ToolManager();
        static readonly HotkeyContext HotkeyContext = new("Targeting");
        public static readonly HotkeyContext HotkeyContextDebug = new("Debug");

        static ToolManager()
        {
            HotkeyManager.RegisterHotkey(HotkeyContext, "Ignore entity mouse hover", ToggleEntityTargeting, System.Windows.Forms.Keys.V);
            HotkeyManager.RegisterHotkey(HotkeyContextDebug, "Toggle draw regions", ToggleDrawRegions, System.Windows.Forms.Keys.F7);
            HotkeyManager.RegisterHotkey(HotkeyContext, "Rotate camera left", delegate { Ingame.CurrentMap.Camera.RotateClockwise(); }, System.Windows.Forms.Keys.Oemcomma);
            HotkeyManager.RegisterHotkey(HotkeyContext, "Rotate camera right", delegate { Ingame.CurrentMap.Camera.RotateCounterClockwise(); }, System.Windows.Forms.Keys.OemPeriod);

            HotkeyManager.RegisterHotkey(HotkeyContext, "Rotate construction clockwise", () => CurrentTool.RotateClockwise(), System.Windows.Forms.Keys.E);
            HotkeyManager.RegisterHotkey(HotkeyContext, "Rotate construction anticlockwise", () => CurrentTool.RotateAntiClockwise(), System.Windows.Forms.Keys.Q);
        }

        readonly Type DefaultToolType = typeof(ToolManagement);
        internal ControlTool GetDefaultTool()
        {
            return Activator.CreateInstance(DefaultToolType) as ControlTool;// new ToolManagement();
        }
        public static ControlTool CurrentTool => Instance.ActiveTool;
        ControlTool _activeTool;
        public ControlTool ActiveTool
        {
            get => this._activeTool ??= this.GetDefaultTool();
            set
            {
                this._activeTool?.CleanUp();
                this._activeTool = value ?? this.GetDefaultTool();
                if (this._activeTool is not null)
                {
                    this._activeTool.Manager = this;
                    var net = Ingame.CurrentMap.Net;
                    PacketPlayerToolSwitch.Send(net, net.GetPlayer().ID, this._activeTool);
                }
                if (ToolHelpTextGui is null)
                {
                    ToolHelpTextGui = new GroupBox(256, 32);
                }
                if (!this._activeTool.HelpText.IsNullEmptyOrWhiteSpace())
                {
                    // show movable window with tool help text
                    ToolHelpTextGui.ClearControls();
                    ToolHelpTextGui.AddControls(new Label(this._activeTool.HelpText) { MouseThrough = true, BackgroundColor = Color.Black * .5f });
                    ToolHelpTextGui.Show();
                }
                else
                    ToolHelpTextGui.Hide();
            }
        }

        public void Update(MapBase map, SceneState scene)
        {
            this.ActiveTool?.Update(scene);
            foreach (var pl in GetOtherPlayers(map))
                pl.CurrentTool?.UpdateRemote(pl.Target);
        }

        internal void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera camera)
        {
            if (this.ActiveTool is null)
                return;
            this.ActiveTool.DrawBeforeWorld(sb, map, camera);
        }
        internal void DrawAfterWorld(MySpriteBatch sb, MapBase map)
        {
            //var map = Ingame.GetMap();
            var camera = map.Camera;
            if (this.ActiveTool is null)
                return;
            this.ActiveTool.DrawAfterWorld(sb, map);
            sb.Flush();
            //DrawPlayersBlockMouseover(sb, map);
        }

        private static void DrawPlayersBlockMouseover(MySpriteBatch sb, MapBase map)
        {
            var camera = map.Camera;
            foreach (var pl in GetOtherPlayers(map))
                if (pl.CurrentTool is not null)
                    if (pl.Target.Type == TargetType.Position)
                        camera.DrawBlockMouseover(sb, map, pl.Target.Global, pl.Color);
        }

        internal void DrawUI(SpriteBatch sb, MapBase map)
        {
            if (this.ActiveTool is null)
                return;
            sb.Begin();
            var camera = map.Camera;
            this.DrawPlayerMousePointers(sb, map);
            this.ActiveTool.DrawUI(sb, camera);
            foreach (var pl in GetOtherPlayers(map))
                if (pl.CurrentTool is not null && pl.Target.Type == TargetType.Entity && pl.Target.Exists)
                        pl.Target.Object.GetScreenBounds(camera).DrawHighlightBorder(sb, pl.Color, camera.Zoom);
            sb.End();
        }
        internal void DrawPlayerMousePointers(SpriteBatch sb, MapBase map)
        {
            var camera = map.Camera;
            foreach (var pl in GetOtherPlayers(map))
            {
                pl.Target.Map = map;
                if (pl.Target.Type != TargetType.Null && pl.Target.Exists)
                {
                    Vector2 pos = GetSmoothedMousePosition(camera, pl);
                    Icon.CursorGrayscale.Draw(sb, new Vector2((int)pos.X, (int)pos.Y), pl.Color);
                    pl.CurrentTool.DrawIcon(sb, pos + new Vector2(Icon.Cursor.AtlasToken.Rectangle.Width, 0));
                    UIManager.DrawStringOutlined(sb, pl.CurrentTool.GetType().Name, pos, new Vector2(.5f, 1));
                    UIManager.DrawStringOutlined(sb, pl.Name, pos, new Vector2(.5f, 2));
                }
            }
        }

        private static Vector2 GetSmoothedMousePosition(Camera camera, PlayerData pl)
        {
            Vector2 pos = camera.GetScreenPosition(pl.Target);
            if (pl.LastPointer.HasValue)
            {
                var lastpos = pl.LastPointer.Value;
                var diff = pos - lastpos;
                pos = lastpos + diff * .1f;
            }
            pl.LastPointer = pos;
            return pos;
        }

        public void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.Handled)
                return;
            this.ActiveTool?.HandleKeyPress(e);
        }

        public void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;
            if (HotkeyManager.Release(e.KeyCode, HotkeyContext) ||
               HotkeyManager.Release(e.KeyCode, HotkeyContextDebug))
            {
                e.Handled = true;
                return;
            }
            this.ActiveTool?.HandleKeyUp(e);
        }

        public void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;

            if (this.ActiveTool is null)
                return;

            if (HotkeyManager.Press(e.KeyCode, HotkeyContext) ||
                HotkeyManager.Press(e.KeyCode, HotkeyContextDebug))
            {
                e.Handled = true;
                return;
            }

            this.ActiveTool.HandleKeyDown(e);
        }

        private static void ToggleEntityTargeting()
        {
            Controller.BlockTargeting = !Controller.BlockTargeting;
            Client.Instance.ConsoleBox.Write("Block targeting " + (Controller.BlockTargeting ? "on" : "off"));
        }
        private static void ToggleDrawRegions()
        {
            Engine.DrawRegions = !Engine.DrawRegions;
        }
        public void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.ActiveTool?.HandleMouseMove(e);
        }

        public void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool is null)
                return;
            if (this.ActiveTool.MouseLeftPressed(e) == ControlTool.Messages.Remove)
                this.ActiveTool = this.GetDefaultTool();
        }

        public void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool is null)
                return;
            if (this.ActiveTool.MouseLeftUp(e) == ControlTool.Messages.Remove)
                this.ActiveTool = this.GetDefaultTool();
        }

        public void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool is null)
                return;
            if (this.ActiveTool.MouseRightDown(e) == ControlTool.Messages.Remove)
                this.ActiveTool = this.GetDefaultTool();
        }

        public void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool is null)
                return;
            if (this.ActiveTool.MouseRightUp(e) == ControlTool.Messages.Remove)
                this.ActiveTool = this.GetDefaultTool();
        }
        public void HandleMButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool is null)
                return;
            if (this.ActiveTool.MouseMiddleUp(e) == ControlTool.Messages.Remove)
                this.ActiveTool = this.GetDefaultTool();
        }
        public void HandleMiddleDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool is null)
                return;
            if (this.ActiveTool.MouseMiddleDown(e) == ControlTool.Messages.Remove)
                this.ActiveTool = this.GetDefaultTool();
        }
        public void HandleMiddleUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool is null)
                return;
            if (this.ActiveTool.MouseMiddleUp(e) == ControlTool.Messages.Remove)
                this.ActiveTool = this.GetDefaultTool();
        }
        public void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.ActiveTool?.HandleMouseWheel(e);
        }
        public void HandleLButtonDoubleClick(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.ActiveTool?.HandleLButtonDoubleClick(e);
        }

        static readonly ToolControlActor ToolControlActor = new();
        internal static void OnGameEvent(WorldBase world, GameEvent e)
        {
            Instance.ActiveTool?.OnGameEvent(e);
            switch (e.Type)
            {
                case Message.Types.PlayerControlNpc:
                    if (e.Parameters[0] == Client.Instance.GetPlayer())
                    {
                        var actor = e.Parameters[1] as GameObject;
                        if (Engine.Map != actor.Map)
                            throw new Exception();
                        Engine.Map.Camera.ToggleFollowing(actor);
                        if (actor is not null)
                            SetTool(ToolControlActor);
                    }
                    break;

                default:
                    break;
            }

        }
        static Control ToolHelpTextGui;
        public static void SetTool(ControlTool tool)
        {
            ScreenManager.CurrentScreen.ToolManager.ActiveTool = tool;
           
        }
        public static bool Clear()
        {
            if(Instance.ActiveTool.GetType() != Instance.DefaultToolType)
            {
                Instance.ActiveTool = null;
                return true;
            }
            return false;
        }

        public static TargetArgs CurrentTarget => Instance.ActiveTool != null ? (Instance.ActiveTool.Target ?? TargetArgs.Null) : TargetArgs.Null;

        public static TargetArgs LastValidTarget
        {
            get
            {
                if (Instance.ActiveTool is not null)
                {
                    if (Instance.ActiveTool.Target != null && Instance.ActiveTool.Target.Type != TargetType.Null)
                        return Instance.ActiveTool.Target;
                    else
                        return Instance.ActiveTool.TargetLast;
                }
                return null;
            }
        }
      
        private static IEnumerable<PlayerData> GetOtherPlayers(MapBase map)
        {
            return map.Net.GetPlayers();
        }
    }
}
