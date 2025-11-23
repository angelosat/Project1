using System;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.PlayerControl
{
    public class DefaultTool : ControlTool
    {
        bool MoveToggle = false;
        bool RButtonDown = false;

        bool MouseMove = true;
        GameObject Actor => Client.Instance.GetPlayer().ControllingEntity;

        public DefaultTool()
        {
        }

        protected virtual void SwitchTool()
        {
            this.StopWalking();
            ToolManager.SetTool(new ToolManagement());
            Client.Instance.ConsoleBox.Write("Switched to management mode");
        }

        protected void StopWalking()
        {
            if (!Walking)
                return;
            if (MoveToggle)
                return;
            Walking = false;
        }

        
        public override void Update()
        {
            base.Update();
            if (MoveToggle)
                MoveMouse();
            else
                MoveKeys();
        }

        public virtual void MoveKeys()
        {
            int xx = 0, yy = 0;

            if (Up)
            {
                xx -= 1;
                yy -= 1;
            }
            else if (Down)
            {
                yy += 1;
                xx += 1;
            }
            if (Left)
            {
                yy += 1;
                xx -= 1;
            }
            else if (Right)
            {
                yy -= 1;
                xx += 1;
            }
          
            if (xx != 0 || yy != 0)
            {
                var actor = this.Actor;
                var cam = actor.Map.Camera;

                double rx, ry;
                double cos = Math.Cos((-cam.Rotation) * Math.PI / 2f);
                double sin = Math.Sin((-cam.Rotation) * Math.PI / 2f);
                rx = (xx * cos - yy * sin);
                ry = (xx * sin + yy * cos);
                int roundx, roundy;
                roundx = (int)Math.Round(rx);
                roundy = (int)Math.Round(ry);

                var NextStep = new Vector2(roundx, roundy);
                NextStep.Normalize();
                var direction = new Vector3(NextStep.X, NextStep.Y, actor.Velocity.Z);

                Walking = true;

            }
            else
            {
                StopWalking();
            }
        }

        public override void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            base.HandleMouseMove(e);
        }

        bool Walking = false;
        bool Up, Down, Left, Right;
        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;
            if (e.KeyCode == GlobalVars.KeyBindings.North || e.KeyCode == System.Windows.Forms.Keys.Up)
                Up = true;
            if (e.KeyCode == GlobalVars.KeyBindings.South || e.KeyCode == System.Windows.Forms.Keys.Down)
                Down = true;
            if (e.KeyCode == GlobalVars.KeyBindings.West || e.KeyCode == System.Windows.Forms.Keys.Left)
                Left = true;
            if (e.KeyCode == GlobalVars.KeyBindings.East || e.KeyCode == System.Windows.Forms.Keys.Right)
                Right = true;
        }
        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if(e.Handled)
                return;
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.Up:
                case System.Windows.Forms.Keys.W:
                    Up = false;
                    break;
                case System.Windows.Forms.Keys.Left:
                case System.Windows.Forms.Keys.A:
                    Left = false;
                    break;
                case System.Windows.Forms.Keys.Right:
                case System.Windows.Forms.Keys.D:
                    Right = false;
                    break;
                case System.Windows.Forms.Keys.Down:
                case System.Windows.Forms.Keys.S:
                    Down = false;
                    break;
                case System.Windows.Forms.Keys.Z:
                    break;
                case System.Windows.Forms.Keys.X:
                    break;
                default:
                    break;
            }
            if (!(Up || Down || Left || Right))
                StopWalking();

            if (e.KeyCode == System.Windows.Forms.Keys.M)
            {
                this.MouseMove = !this.MouseMove;
                Client.Instance.ConsoleBox.Write("Mouse move " + (this.MouseMove ? "Enabled" : "Disabled"));
            }

            base.HandleKeyUp(e);
        }

        internal override void ManageEquipment()
        {
            if (!Controller.Instance.Mouseover.TryGet(out GameObject _))
                return;
        }

        public Messages OnKey(System.Windows.Forms.Keys key)
        {
            if (!Controller.Instance.Mouseover.TryGet(out GameObject _))
                return Messages.Default;
            return Messages.Default;
        }

        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;

            if (this.MouseMove)
            {
                MoveToggle = true;
            }
            return Messages.Default;
        }

        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.MouseMove)
            {
                MoveToggle = false;
                StopWalking();
            }
            return base.MouseLeftUp(e);
        }

        public void MoveMouse()
        {
            Walking = true;
        }
        public static Vector2 GetDirection2()
        {
            var cam = Engine.Map.Camera;
            int x = Controller.Instance.msCurrent.X - cam.Width / 2;
            int y = Controller.Instance.msCurrent.Y - cam.Height / 2;
            Coords.Ortho(x, y, out float xx, out float yy);
            Coords.Rotate((int)cam.Rotation, xx, yy, out int xxx, out int yyy);
            var normal = new Vector2(xxx, yyy);
            normal.Normalize();
            var final = new Vector2(normal.X, normal.Y);
            return final;
        }
        public static Vector3 GetDirection3(GameObject referenceEntity)
        {
            var cam = referenceEntity.Map.Camera;
            var playerScreenPosition = cam.GetScreenPosition(referenceEntity.Global);
            int x = Controller.Instance.msCurrent.X - (int)playerScreenPosition.X;
            int y = Controller.Instance.msCurrent.Y - (int)playerScreenPosition.Y;
            Coords.Ortho(x, y, out float xx, out float yy);
            Coords.Rotate((int)cam.Rotation, xx, yy, out int xxx, out int yyy);
            var normal = new Vector2(xxx, yyy);
            normal.Normalize();
            var final = new Vector3(normal.X, normal.Y, 0);
            return final;
        }
        
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;
            this.RButtonDown = true;
            return base.MouseRightDown(e);
        }
        public override Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.RButtonDown)
                return Messages.Default;
            this.RButtonDown = false;
            if (ContextMenu2.Instance.IsOpen)
                return Messages.Default;

            // maybe just create a "direction" target if the current one is null?

            RightClick();
            return Messages.Default;
        }
        
        private void RightClick()
        {
            if (this.Target is null)
                return;
            var action = this.Target.GetContextRB(this.Actor);

            if (action is not null)
                if (action.Action is not null)
                {
                    action.Action();
                    return;
                }
            if (!InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                this.Target.Precise = Vector3.Zero;
        }

        internal override void GetContextActions(ContextArgs args)
        {
            this.Target.GetContextAll(this.Actor, args);
        }

        internal override void DrawAfterWorld(MySpriteBatch sb, MapBase map)
        {
            var cam = map.Camera;
            base.DrawAfterWorld(sb, map);
            var haul = this.Actor.Inventory.HaulSlot.Object;
            if (haul is not null)
            {
                if (this.Target is null)
                    return;
                var global = this.Target.Global + this.Target.Face + (!InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : this.Target.Precise);
                haul.DrawPreview(sb, cam, global);
            }
        }
    }
}
