using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.AI.Packets;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Graphics;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.UI.Hud;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Input;
using Project1.Framework.UI;
using System;
using System.Linq;
using System.Windows.Forms;
using Button = Project1.Framework.UI.Button;
using Control = Project1.Framework.UI.Control;
using Panel = Project1.Framework.UI.Panel;

namespace Project1.Core.Input
{
    [EnsureStaticCtorCall]
    public class ToolManagement : DefaultTool
    {
        static bool Up, Down, Left, Right;
        private DateTime MouseMiddleTimestamp;
        Vector2 MouseScrollOrigin;
        Vector2 CameraCoordinatesOrigin;
        Action ScrollingMode;
        public static readonly HotkeyContext HotkeyContextManagement = new("Management");
        protected HotkeyContext HotkeyContext => HotkeyContextManagement;
        static ToolManagement()
        {
            HotkeyManager.RegisterHotkey(HotkeyContextManagement, "Pause/Resume", PauseResume, Keys.Space);
            HotkeyManager.RegisterHotkey(HotkeyContextManagement, "Speed: Normal", delegate { SetSpeed(1); }, Keys.D1);
            HotkeyManager.RegisterHotkey(HotkeyContextManagement, "Speed: Fast", delegate { SetSpeed(2); }, Keys.D2);
            HotkeyManager.RegisterHotkey(HotkeyContextManagement, "Speed: Faster", delegate { SetSpeed(3); }, Keys.D3);
            HotkeyToggleForbidden = HotkeyManager.RegisterHotkey(HotkeyContextManagement, "Toggle Forbidden", ToggleForbidden, Keys.F);
            HotkeyManager.RegisterHotkey(HotkeyContextManagement, "Set draw elevation to selection", Slice, Keys.Z);

            HotkeyManager.RegisterHotkey(HotkeyContextManagement, "Camera: Up", () => Up = true, () => Up = false, Keys.W, Keys.Up);
            HotkeyManager.RegisterHotkey(HotkeyContextManagement, "Camera: Down", () => Down = true, () => Down = false, Keys.S, Keys.Down);
            HotkeyManager.RegisterHotkey(HotkeyContextManagement, "Camera: Left", () => Left = true, () => Left = false, Keys.A, Keys.Left);
            HotkeyManager.RegisterHotkey(HotkeyContextManagement, "Camera: Right", () => Right = true, () => Right = false, Keys.D, Keys.Right);

            HotkeyCameraFaster = HotkeyManager.RegisterHotkey(HotkeyContextManagement, "Faster camera speed", delegate { }, Keys.ShiftKey);
        }
        internal static readonly IHotkey HotkeyToggleForbidden, HotkeyCameraFaster;

        public ToolManagement()
        {
        }
        public override Icon GetIcon()
        {
            return null;
        }
        TargetArgs Origin;
        Vector2? SelectionRectangleOrigin;
        bool LeftPressed, DblClicked;

        public override void Update()
        {
            var map = Ingame.GetMap();
            var cam = map.Camera;
            cam.MousePicking(map, this.TargetOnlyBlocks);
            this.UpdateTargetNew();

            if (this.Origin is not null && this.Target is not null && this.Origin.Global != this.Target.Global)
            {
                ToolManager.SetTool(new ToolSelect(this.Origin));
                this.Origin = null;
                return;
            }
            if (this.SelectionRectangleOrigin.HasValue &&
                Vector2.DistanceSquared(this.SelectionRectangleOrigin.Value, UIManager.Mouse) > 50)
            {
                ToolManager.SetTool(new ToolSelectRectangle(this.SelectionRectangleOrigin.Value));
                this.SelectionRectangleOrigin = null;
                return;
            }
            if (this.ScrollingMode is not null)
            {
                this.ScrollingMode();
            }
            else
                this.MoveKeys();

            this.OnUpdate();
        }
        protected virtual void OnUpdate() { }

        static int LastSpeed = 1;
        internal override void Jump()
        {
            PauseResume();
        }
        static int _lastSpeed;
        static void PauseResume()
        {
            if (_lastSpeed == 0)
                _lastSpeed = 1;
            else _lastSpeed = 0;
            var nextSpeed = _lastSpeed;
            Ingame.Instance.Events.Post(new PlayerChangedSpeedEvent(nextSpeed));
        }

        private void ClickTarget(TargetArgs target)
        {
            if (target.Type == TargetType.Cell)// || target.Type == TargetType.BlockEntity)
            {
                IntVec3 global = target.Global;
                if (target.Map.TryGetBlockEntity(target.Global, out var blockEntity))
                    Ingame.Instance.Events.Post(new PlayerSelectionSingleEvent(Single: new TargetArgs(blockEntity)));
                else
                    Ingame.Instance.Events.Post(new PlayerSelectionCubeEvent(global, global));
            }
            else if(target.Type == TargetType.Entity)
            {
                Ingame.Instance.Events.Post(new PlayerSelectionRectangleEvent([target.Entity as Entity], SelectionHelper.GetSelectionOp()));
            }
            //else if (target.Type == TargetType.BlockEntity)
            //{
            //    Ingame.Instance.Events.Post(new PlayerSelectionSingleEvent(Single: new TargetArgs(target.BlockEntity)));
            //}
            return;
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey))
                //SelectionManager.AddToSelection(target);
                //Ingame.Instance.Events.Post(new PlayerSelectionRectangleEvent([target], SelectionOp.Add));
                Ingame.Instance.Events.Post(new PlayerSelectionSingleEvent(Add: target));
            else
            {
                if (target.Type == TargetType.Cell)
                {
                    if (target.Map.TryGetBlockEntity(target.Global, out var blockEntity))
                    {
                        Ingame.Instance.Events.Post(new PlayerSelectionSingleEvent(Single: new TargetArgs(blockEntity)));
                        return;
                    }
                }
                Ingame.Instance.Events.Post(new PlayerSelectionSingleEvent(Single: target));
            }
        }
        
        private void MouseScroll()
        {
            var currentMouse = UIManager.Mouse;
            var delta = currentMouse - this.MouseScrollOrigin;
            var l = delta.Length();
            if (l < 5)
                return;
            l -= 5;

            delta.Normalize();
            var minL = Math.Min(Math.Max(l, 1), 500);
            delta *= minL;

            delta *= .01f;
            var cam = Engine.Map.Camera;
            cam.Move(cam.Coordinates += delta * 4);

        }
        private void MouseDrag()
        {
            var currentMouse = UIManager.Mouse;
            var delta = currentMouse - this.MouseScrollOrigin;
            var map = Ingame.GetMap();
            var cam = map.Camera;
            cam.Move(this.CameraCoordinatesOrigin - delta / cam.Zoom);
        }

        public override void MoveKeys()
        {
            int xx = 0, yy = 0;

            if (Up)
                yy -= 1;
            else if (Down)
                yy += 1;
            if (Left)
                xx -= 1;
            else if (Right)
                xx += 1;
            if (xx != 0 || yy != 0)
            {
                var cam = Ingame.CurrentMap.Camera;

                double rx, ry;
                double cos = Math.Cos((-cam.Rotation) * Math.PI / 2f);
                double sin = Math.Sin((-cam.Rotation) * Math.PI / 2f);
                rx = xx * cos - yy * sin;
                ry = xx * sin + yy * cos;
                int roundx, roundy;
                roundx = (int)Math.Round(rx);
                roundy = (int)Math.Round(ry);

                var nextStep = new Vector2(roundx, roundy);
                nextStep.Normalize();

                var speed = HotkeyCameraFaster.ShortcutKeys.Any(k => InputState.IsKeyDown(k)) ? 3 : 1;
                cam.Move(cam.Coordinates += new Vector2(xx, yy) * 4 * speed);
            }
        }
        public override void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Handled)
                return;
            e.Handled = HotkeyManager.Press(e.KeyCode, this.HotkeyContext);
        }
        private static void ToggleForbidden()
        {
            var targets = SelectionManager.GetSelectedEntities().Where(o => o.IsForbiddable());
            //PacketToggleForbidden.Send(Client.Instance, SelectionManager.GetSelectedEntities().Where(o => o.IsForbiddable()));
            Ingame.Instance.Events.Post(new PlayerForbiddingItemsEvent([.. targets.Cast<Entity>()]));
        }

        private static void SetSpeed(int value)
        {
            if(value != 0)
                _lastSpeed = value;
            Ingame.Instance.Events.Post(new PlayerChangedSpeedEvent(value));
        }

        public override void HandleKeyUp(KeyEventArgs e)
        {
            if (e.Handled)
                return;
            e.Handled = HotkeyManager.Release(e.KeyCode, this.HotkeyContext);
        }
        public override void HandleMouseWheel(HandledMouseEventArgs e)
        {
            base.HandleMouseWheel(e);
            var map = Ingame.GetMap();
            var cam = map.Camera;
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.LControlKey))
            {
                cam.AdjustDrawLevel(InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey) ? e.Delta * 16 : e.Delta);
                return;
            }
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu))
            {
                cam.Rotation += e.Delta;
                return;
            }
            if (e.Delta < 0)
                cam.ZoomDecrease();
            else
                cam.ZoomIncrease();

        }
        public override Messages MouseLeftPressed(HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            this.LeftPressed = true;
            this.SelectionRectangleOrigin = UIManager.Mouse;
            //e.Handled = true;
            return Messages.Default;
        }
        public override Messages MouseLeftUp(HandledMouseEventArgs e)
        {
            if (this.DblClicked)
            {
                this.DblClicked = false;
                return base.MouseLeftUp(e);
            }
            if (!e.Handled && this.LeftPressed)
                if (this.Target.Type != TargetType.Null)
                    this.ClickTarget(this.Target);
            this.Origin = null;
            this.SelectionRectangleOrigin = null;
            this.LeftPressed = false;
            return base.MouseLeftUp(e);
        }
        public override Messages MouseMiddleDown(HandledMouseEventArgs e)
        {
            if (this.ScrollingMode != this.MouseScroll)
                this.MouseMiddleTimestamp = DateTime.Now;
            this.ScrollingMode = this.MouseDrag;
            this.MouseScrollOrigin = UIManager.Mouse;
            var map = Ingame.GetMap();
            var cam = map.Camera;
            this.CameraCoordinatesOrigin = cam.Coordinates;
            return Messages.Default;
        }
        public override Messages MouseMiddleUp(HandledMouseEventArgs e)
        {
            var d = DateTime.Now - this.MouseMiddleTimestamp;
            var c = TimeSpan.FromMilliseconds(100);
            const int mouseScrollDistanceThreshold = 5;
            if (d < c && this.ScrollingMode != this.MouseScroll && Vector2.DistanceSquared(UIManager.Mouse, this.MouseScrollOrigin) < mouseScrollDistanceThreshold)
                this.ScrollingMode = this.MouseScroll;
            else
                this.ScrollingMode = null;
            return Messages.Default;
        }
        public override Messages MouseMiddle()
        {
            return base.MouseMiddle();
        }
        public override Messages MouseRightDown(HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;

            if (!this.TryShowForceTaskGUI(this.Target))
                Ingame.CurrentMap.Town.ToggleQuickMenu();

            e.Handled = true;
            return Messages.Default;
        }
        public override Messages MouseRightUp(HandledMouseEventArgs e)
        {
            return Messages.Default;
        }

        public override void HandleLButtonDoubleClick(HandledMouseEventArgs e)
        {
            if (this.Target != null)
            {
                if (this.Target.Type == TargetType.Entity)
                    SelectionManager.SelectAllVisible(this.Target.Object.Def);

                else if (this.Target.Type == TargetType.Cell && !this.Target.TryGetBlockEntity(out _))
                    ToolManager.SetTool(
                        new ToolSelectRectangleBlocks(this.Target.Global,
                        (a, b, r) =>
                        {
                            //if (a == b)
                            //    SelectionManager.Select(this.Target);
                            //else
                            //{
                                Ingame.Instance.Events.Post(new PlayerSelectionCubeEvent(a, b));
                                //SelectionManager.Instance.Select(new SelectionIntent(a, b));
                            //}
                        }));
            }
            this.DblClicked = true;
            e.Handled = true;
        }
       
        private bool TryShowForceTaskGUI(TargetArgs target)
        {
            var actor = SelectionManager.SingleSelectedEntity as Actor;

            if (!(actor?.IsTownMember ?? false))
                return false;

            var taskGivers = actor.CanForceTaskOn(target);
            if (taskGivers.Any())
            {
                UIForceTask.ClearControls();
                UIForceTask.AddControlsBottomLeft(taskGivers
                    .Select(result =>
                    {
                        return new Button(result.task.GetForceText(target))
                        {
                            LeftClickAction = () =>
                            {
                                PacketForceTask.Send(result.giver, actor, target);
                                UIForceTask.Hide();
                            }
                        };

                    }).ToArray());

                UIForceTask.Location = UIManager.Mouse;
                UIForceTask.Show();
                return true;
            }
          
            return false;
        }

        static readonly Control UIForceTask = new Panel() { AutoSize = true }.HideOnAnyClick();

        internal override void DrawAfterWorld(MySpriteBatch sb, MapBase map)
        {
            var cam = map.Camera;
            if(this.Target is TargetArgs tar && tar.Type == TargetType.Cell)
                cam.DrawBlockMouseover(sb, map, tar, Color.White);
            if (this.Target is null || this.Target.Type == TargetType.Null)
                return;
            if (Engine.DrawRegions && this.Target.Type != TargetType.Null)
                map.Regions.Draw(this.Target.Global, sb, cam);
        }
        internal override void DrawUI(SpriteBatch sb, Camera camera)
        {
            if (this.ScrollingMode == this.MouseScroll)
                Icon.Cross.Draw(sb, this.MouseScrollOrigin, Vector2.One * .5f);
            base.DrawUI(sb, camera);
        }

        public static void Slice()
        {
            if (ToolManager.Instance.ActiveTool is not ToolManagement)
                return;
            ScreenManager.CurrentScreen.Camera.SliceOn((int)SelectionManager.Instance.SelectedSource.Global.Z);
        }
    }
}