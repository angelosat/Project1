using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.UI;

namespace Start_a_Town_.Modules.Construction
{
    public partial class ToolPlaceWall : DefaultTool
    {
        public enum Modes { Wall, Single }
        public Modes Mode;
        Action<Args> Callback;
        bool Enabled, Valid, SettingHeight;
        Vector3 Begin, End, Axis;
        int Height;

        public ToolPlaceWall(Action<Args> callback)
        {
            this.Callback = callback;
        }

        private void CheckValidity()
        {
            this.Valid = true;
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if(this.SettingHeight)
            {
                var args = new Args(this.Begin, this.End + Vector3.UnitZ * this.Height, InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey));
                this.Callback(args);
                this.SettingHeight = false;
                this.Enabled = false;
                return Messages.Default;
            }
            if (this.Enabled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            if (this.Target.Type != TargetType.Position)
                return Messages.Default;
            var pos = this.Target.FaceGlobal;
            this.Begin = pos;
            this.End = this.Begin;
            this.Height = 0;
            this.Enabled = true;
            return Messages.Default;
        }

        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            var target = this.Target;
            if (target == null)
                return ControlTool.Messages.Default;
            if (IsRemoving())
            {

            }
            CheckValidity();
            
            if(this.Mode == Modes.Single)
            {
                var args = new Args(this.Begin, this.End + Vector3.UnitZ * this.Height, InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey));
                this.Callback(args);
                this.Enabled = false;
                return ControlTool.Messages.Default;
            }
            if (this.SettingHeight)
            {
                this.Enabled = false;
                this.SettingHeight = false;
            }
            else
                this.SettingHeight = this.Enabled;
            return ControlTool.Messages.Default;
        }

        public override void Update()
        {
            base.Update();
            if (!Enabled)
                return;
            if (this.Target == null)
                return;
            if (this.Target.Type != TargetType.Position)
                return;
            if(this.SettingHeight)
            {
                this.SetHeight();
                return;
            }
            if (this.Mode == Modes.Single)
                return;
            var end = this.Target.Global;
            var dx = end.X - this.Begin.X;
            var adx = Math.Abs(dx);
            var dy = end.Y - this.Begin.Y;
            var ady = Math.Abs(dy);
            if (adx > ady)
                this.Axis = Vector3.UnitX + Vector3.UnitZ;
            else if (ady > adx)
                this.Axis = Vector3.UnitY + Vector3.UnitZ;

            this.End = this.Begin + new Vector3(dx * this.Axis.X, dy * this.Axis.Y, 0);
        }

        private void SetHeight()
        {
            var endscreenposition = ScreenManager.CurrentScreen.Camera.GetScreenPosition(this.End);
            var currentposition = UIManager.Mouse;
            var length = Math.Max(0, endscreenposition.Y - currentposition.Y) / ScreenManager.CurrentScreen.Camera.Zoom;
            var lengthinblocks = (int)(length / Block.BlockHeight);
            this.Height = lengthinblocks;
        }

        public override ControlTool.Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera camera)
        {
            camera.Effect.Parameters["s"].SetValue(Sprite.Atlas.Texture);
            camera.Effect.Parameters["s1"].SetValue(Sprite.Atlas.DepthTexture);
            this.DrawGrid(sb, camera);

            base.DrawBeforeWorld(sb, map, camera);
        }
        void DrawGrid(MySpriteBatch sb, Camera cam)
        {
            if (!this.Enabled)
                return;
            var end = this.End + Vector3.UnitZ * this.Height;
            var col = this.Valid ? Color.Lime : Color.Red;
            int x = (int)Math.Min(this.Begin.X, end.X);
            int y = (int)Math.Min(this.Begin.Y, end.Y);
            int z = (int)Math.Min(this.Begin.Z, end.Z);

            int dx = (int)Math.Abs(this.Begin.X - end.X);
            int dy = (int)Math.Abs(this.Begin.Y - end.Y);
            int dz = (int)Math.Abs(this.Begin.Z - end.Z);

            var minBegin = new Vector3(x, y, z);
            for (int i = 0; i <= dx; i++)
            {
                for (int j = 0; j <= dy; j++)
                {
                    for (int k = 0; k <= dz; k++)
                    {
                        Vector3 global = minBegin + new Vector3(i, j, k);

                        var bounds = cam.GetScreenBounds(global, Block.Bounds);
                        var pos = new Vector2(bounds.X, bounds.Y);
                        var depth = global.GetDrawDepth(Engine.Map, cam);
                        sb.Draw(Sprite.Atlas.Texture, pos, Sprite.BlockHighlight.AtlasToken.Rectangle, 0, Vector2.Zero, cam.Zoom, col * .5f, SpriteEffects.None, depth);
                    }
                }
            }
        }

        internal override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        {
            base.DrawUI(sb, camera);
            if (IsDesignating())
                Icon.Replace.Draw(sb);
            else if(IsRemoving())
                Icon.Cross.Draw(sb);
        }

        private static bool IsDesignating()
        {
            return InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey);
        }
        private static bool IsRemoving()
        {
            return InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey);
        }
    }
}
