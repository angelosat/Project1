using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    class ToolSelect3D : ControlTool
    {
        enum ValidityType { Invalid, Valid, Ignore }
        protected IntVec3 Begin, End;
        int Width, Height;
        protected bool Enabled;
        bool Removing;
        protected Action<Vector3, Vector3, bool> Add;
        Func<List<Vector3>> GetZones = () => new List<Vector3>();

        protected ToolSelect3D()
        {

        }
       
        public ToolSelect3D(Action<Vector3, Vector3, bool> callback)
            : this(callback, () => new List<Vector3>())
        {
        }
        public ToolSelect3D(Action<Vector3, Vector3, bool> callback, Func<List<Vector3>> zones)
        {
            this.Add = callback;
            this.GetZones = zones;
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

            this.End = (IntVec3)this.Target.Global;

            var w = (int)Math.Abs(this.Target.Global.X - this.Begin.X) + 1;
            var h = (int)Math.Abs(this.Target.Global.Y - this.Begin.Y) + 1;
            this.Width = w;
            this.Height = h;
        }
        public override void UpdateRemote(TargetArgs target)
        {
            base.UpdateRemote(target);
            if (!Enabled)
                return;
            if (this.Target == null)
                return;
            if (this.Target.Type != TargetType.Position)
                return;

            this.End = this.Target.Global;

            var w = (int)Math.Abs(this.Target.Global.X - this.Begin.X) + 1;
            var h = (int)Math.Abs(this.Target.Global.Y - this.Begin.Y) + 1;
            this.Width = w;
            this.Height = h;
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
            if (this.GetZones().Contains(pos))
                this.Removing = true;
            this.Begin = pos;
            this.End = this.Begin;
            this.Width = this.Height = 1;
            this.Enabled = true;
            this.Sync();
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
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            int z = (int)Math.Min(this.Begin.Z, this.End.Z);

            int xx = (int)(this.Begin.X + this.End.X - x);
            int yy = (int)(this.Begin.Y + this.End.Y - y);
            int zz = (int)(this.Begin.Z + this.End.Z - z);

            var rect = new Rectangle(x, y, this.Width, this.Height);

            var begin = new Vector3(x, y, z);
            var end = new Vector3(xx, yy, zz);

            this.Add(begin, end, IsRemoving());

            this.Removing = false;
            this.Enabled = false;
            return Messages.Default;
        }

        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Enabled)
            {
                this.Enabled = false;
                this.Sync();
                return Messages.Default;
            }
            else
                return Messages.Remove;
        }

        Icon _Icon = new(UI.UIManager.Icons32, 12, 32);
        public override Icon Icon => _Icon;
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
            int dx = (int)Math.Abs(this.Begin.X - this.End.X + 1);
            int dy = (int)Math.Abs(this.Begin.Y - this.End.Y + 1);
            int dz = (int)Math.Abs(this.Begin.Z - this.End.Z + 1);
            UIManager.DrawStringOutlined(sb, string.Format("{0} x {1} x {2}", dx, dy, dz), UIManager.Mouse, Vector2.UnitY);
        }

        private bool IsRemoving()
        {
            return this.Removing || InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey);
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera camera)
        {
            this.DrawGrid(sb, camera);

            foreach (var g in this.GetZones())
                this.DrawGridCell(sb, camera, Color.Yellow, g);

            base.DrawBeforeWorld(sb, map, camera);
        }

        void DrawGrid(MySpriteBatch sb, Camera cam)
        {
            if (!this.Enabled)
                return;
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            int z = (int)Math.Min(this.Begin.Z, this.End.Z);

            int dx = (int)Math.Abs(this.Begin.X - this.End.X);
            int dy = (int)Math.Abs(this.Begin.Y - this.End.Y);
            int dz = (int)Math.Abs(this.Begin.Z - this.End.Z);

            var minBegin = new Vector3(x, y, z);
            for (int i = 0; i <= dx; i++)
            {
                for (int j = 0; j <= dy; j++)
                {
                    for (int k = 0; k <= dz; k++)
                    {
                        Vector3 global = minBegin + new Vector3(i, j, k);
                        cam.DrawGridBlock(sb, Block.BlockBlueprint, Color.Red, global);
                    }
                }
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
            sb.Draw(Sprite.Atlas.Texture, pos, Sprite.BlockHighlight.AtlasToken.Rectangle, 0, Vector2.Zero, cam.Zoom, col * .5f, SpriteEffects.None, depth);
        }
        protected override void WriteData(BinaryWriter w)
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
