using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class ToolDesignate3D : ToolManagement
    {
        enum ValidityType { Invalid, Valid, Ignore }
        protected IntVec3 Begin, End;
        protected int Width, Height;
        protected bool Enabled;
        bool Valid;
        bool Removing;
        protected Action<IntVec3, IntVec3, bool> Callback;
        public override bool TargetOnlyBlocks => true;

        public ToolDesignate3D()
        {
        }
        public ToolDesignate3D(Action<IntVec3, IntVec3, bool> callback)
        {
            this.Callback = callback;
        }
       
        public override void Update()
        {
            base.Update();
            if (!Enabled)
                return;
            if (this.Target is null)
                return;
            if (this.Target.Type != TargetType.Position)
                return;

            this.End = this.Target.Global;
          
            var w = (int)Math.Abs(this.Target.Global.X - this.Begin.X) + 1;
            var h = (int)Math.Abs(this.Target.Global.Y - this.Begin.Y) + 1;
            if (w != this.Width || h != this.Height)
                this.Valid = this.Check(w, h);
            this.Width = w;
            this.Height = h;
        }

        private bool Check(int w, int h)
        {
            if (w < 1)
                return false;
            if (h < 1)
                return false;
            var positions = this.GetPositions(w, h);
            foreach (var pos in positions)
            {
                if (Engine.Map.IsSolid(pos))
                    return false;
                if (!Engine.Map.IsSolid(pos - IntVec3.UnitZ))
                    return false;
            }
            return true;
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Enabled)
                return Messages.Default;
            if(this.Target == null)
                return Messages.Default;
            if(this.Target.Type != TargetType.Position)
                return Messages.Default;
            var pos = this.Target.Global;
            this.Begin = pos;
            this.End = this.Begin;
            this.Width = this.Height = 1;
            this.Enabled = true;
            this.Valid = this.Check(this.Width, this.Height);
            Sync();
            return Messages.Default;
        }

        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.Enabled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            if (this.Target.Type != TargetType.Position)
                return Messages.Default;
            int x = Math.Min(this.Begin.X, this.End.X);
            int y = Math.Min(this.Begin.Y, this.End.Y);
            int z = Math.Min(this.Begin.Z, this.End.Z);

            int xx = this.Begin.X + this.End.X - x;
            int yy = this.Begin.Y + this.End.Y - y;
            int zz = this.Begin.Z + this.End.Z - z;

            var rect = new Rectangle(x, y, this.Width, this.Height);

            var begin = new IntVec3(x, y, z);
            var end = new IntVec3(xx, yy, zz);

            this.Callback(begin, end, IsRemoving());

            this.Removing = false;
            this.Enabled = false;
            Sync();
            return Messages.Default;
        }

        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Enabled)
            {
                this.Enabled = false;
                Sync();
                return Messages.Default;
            }
            else
                return Messages.Remove;
        }

        List<IntVec3> GetPositions(int w, int h)
        {
            List<IntVec3> list = new List<IntVec3>();
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            for (int i = x; i < x + w; i++)
                for (int j = y; j < y + h; j++)
                    list.Add(new IntVec3(i, j, this.Begin.Z));
            return list;
        }
        Icon _icon = new Icon(UI.UIManager.Icons32, 12, 32);
        public override Icon Icon => this._icon;
        internal override void DrawUI(SpriteBatch sb, Camera camera)
        {
            base.DrawUI(sb, camera); 
            
            Icon.Draw(sb, UI.UIManager.Mouse);
            if (this.IsRemoving())
            {
                var icondelete = Icon.Cross;
                icondelete.Draw(sb, UI.UIManager.Mouse + new Vector2(Icon.SourceRect.Width / 2, 0));
            }
            if (!this.Enabled)
                return;

            int dx = (int)Math.Abs(this.End.X - this.Begin.X) + 1;
            int dy = (int)Math.Abs(this.End.Y - this.Begin.Y) + 1;
            int dz = (int)Math.Abs(this.End.Z - this.Begin.Z) + 1;
            UIManager.DrawStringOutlined(sb, $"{dx} x {dy} x {dz}", UIManager.Mouse, Vector2.UnitY);
        }

        private bool IsRemoving()
        {
            return this.Removing || InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey);
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera camera)
        {
            Block.Atlas.Begin(camera.Effect);
            this.DrawGrid(sb, camera);
            sb.Flush();
            base.DrawBeforeWorld(sb, map, camera);
        }

        void DrawGrid(MySpriteBatch sb, Camera cam)
        {
            if (!this.Enabled)
                return;
            var col = this.Valid ? Color.Lime : Color.Red;
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            int z = (int)Math.Min(this.Begin.Z, this.End.Z);

            int dx = (int)Math.Abs(this.Begin.X - this.End.X);
            int dy = (int)Math.Abs(this.Begin.Y - this.End.Y);
            int dz = (int)Math.Abs(this.Begin.Z - this.End.Z);

            Block.Atlas.Begin(cam.Effect);
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
                        sb.Draw(Block.Atlas.Texture, pos, Block.BlockHighlight.Rectangle, 0, Vector2.Zero, cam.Zoom, col * .5f, SpriteEffects.None, depth);

                    }
                }
            }
        }
        void DrawGrid2d(MySpriteBatch sb, Camera cam)
        {
            if (!this.Enabled)
                return;
            var col = this.Valid ? Color.Lime : Color.Red;
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            for (int i = x; i < x + this.Width; i++)
                for (int j = y; j < y + this.Height; j++)
                {
                    Vector3 global = new Vector3(i, j, this.Begin.Z);

                    var bounds = cam.GetScreenBounds(global, Block.Bounds);
                    var pos = new Vector2(bounds.X, bounds.Y);
                    var depth = global.GetDrawDepth(Engine.Map, cam);
                    sb.Draw(Sprite.Atlas.Texture, pos, Sprite.BlockHighlight.AtlasToken.Rectangle, 0, Vector2.Zero, cam.Zoom, col*.5f, SpriteEffects.None, depth);
                }
        }

        private void DrawGridCell(MySpriteBatch sb, Camera cam, Color col, Vector3 global)
        {
            if (global.Z > cam.MaxDrawZ)
                return;
            var bounds = cam.GetScreenBounds(global, Block.Bounds);
            var pos = new Vector2(bounds.X, bounds.Y);
            var depth = global.GetDrawDepth(Engine.Map, cam);
            if (IsRemoving() && Enabled)
            {
                var x = Math.Min(this.Begin.X, this.End.X);
                var y = Math.Min(this.Begin.Y, this.End.Y);
                var z = Math.Min(this.Begin.Z, this.End.Z);

                var xx = this.Begin.X + this.End.X - x;
                var yy = this.Begin.Y + this.End.Y - y;
                var zz = this.Begin.Z + this.End.Z - z;

                var a = new Vector3(x, y, z);
                var b = new Vector3(xx, yy, zz);
                BoundingBox box = new BoundingBox(a, b);
                if (box.Contains(global) != ContainmentType.Disjoint)
                    col = Color.Red;
            }
            sb.Draw(Block.Atlas.Texture, pos, Block.BlockHighlight.Rectangle, 0, Vector2.Zero, cam.Zoom, col * .5f, SpriteEffects.None, depth);

        }
        protected override void WriteData(System.IO.BinaryWriter w)
        {
            w.Write(this.Enabled);
            w.Write(this.Begin);
        }
        protected override void ReadData(IDataReader r)
        {
            this.Enabled = r.ReadBoolean();
            this.Begin = r.ReadVector3();
        }
    }
}
