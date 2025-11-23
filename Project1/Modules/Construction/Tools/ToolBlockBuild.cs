using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public abstract class ToolBlockBuild : ToolManagement, INamed
    {
        public BuildToolDef ToolDef;
        public string Name => this.ToolDef.Label;
        new readonly Icon Icon = new(UIManager.Icons32, 12, 32);
        public Action<ToolBlockBuild.Args> Callback;
        protected bool Valid, Enabled;
        protected IntVec3 Begin, End, Axis;
        public Block Block;
        public MaterialDef Material;
        public byte State;
        public int Variation, Orientation;
        int Height;
        public override bool TargetOnlyBlocks => true;
        readonly string _helpText = "Hold control to clear designations\nHold shift to replace blocks";
        public override string HelpText =>  _helpText;
        public virtual string Status => this.Enabled ? "Select end point" : "Select start point";

        public ToolBlockBuild()
        {

        }
        public ToolBlockBuild(Action<ToolBlockBuild.Args> callback)
        {
            this.Callback = callback;
        }
        private void CheckValidity()
        {
            this.Valid = true;
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Enabled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            if (this.Target.Type != TargetType.Position)
                return Messages.Default;
            this.Replacing = IsReplacing();
            var pos = this.Replacing ? this.Target.Global : this.Target.FaceGlobal;
            if (!this.IsValid(pos))
                return Messages.Default;
            this.Begin = pos;
            this.End = this.Begin;
            this.Height = 0;
            this.Enabled = true;
            this.Sync();
            return Messages.Default;
        }

        private bool IsValid(IntVec3 pos)
        {
            var map = Ingame.GetMap();
            return map.IsValidBuildSpot(pos, true);
        }

        public override void HandleLButtonDoubleClick(System.Windows.Forms.HandledMouseEventArgs e)
        {
        }
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            var target = this.Target;
            if (target == null)
                return ControlTool.Messages.Default;
            if (IsRemoving())
            {

            }
            this.CheckValidity();

            return ControlTool.Messages.Default;
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Enabled)
            {
                this.Enabled = false;
                this.Replacing = false;
                this.Sync();
                return Messages.Default;
            }
            else
                return Messages.Remove;
        }
        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyValue == 17) //control
            {
                ToolManager.SetTool(new ToolBlockErase(this));
            }
            base.HandleKeyDown(e);
        }
        IntVec3 GetMouseover()
        {
            return IsReplacing() ? this.Target.Global : this.Target.FaceGlobal;
        }
        internal override void DrawAfterWorld(MySpriteBatch sb, MapBase map)
        {
            var cam = map.Camera;
            if (this.Target is not null)
            {
                var vec = this.GetMouseover();
                var col = map.IsValidBuildSpot(vec) ? Color.Lime : Color.Red;
                cam.DrawBlockMouseover(sb, map, vec, col);
            }
            if (this.Enabled)
                this.DrawBlockPreviews(sb, map, cam);
        }

        protected virtual void DrawGrid(MySpriteBatch sb, MapBase map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;

            var end = this.End + IntVec3.UnitZ * this.Height;
            var col = this.Valid ? Color.Lime : Color.Red;
            int x = Math.Min(this.Begin.X, end.X);
            int y = Math.Min(this.Begin.Y, end.Y);
            int z = Math.Min(this.Begin.Z, end.Z);

            int dx = Math.Abs(this.Begin.X - end.X);
            int dy = Math.Abs(this.Begin.Y - end.Y);
            int dz = Math.Abs(this.Begin.Z - end.Z);

            var minBegin = new IntVec3(x, y, z);
            for (int i = 0; i <= dx; i++)
            {
                for (int j = 0; j <= dy; j++)
                {
                    for (int k = 0; k <= dz; k++)
                    {
                        IntVec3 global = minBegin + new IntVec3(i, j, k);
                        cam.DrawGridCell(sb, col, global);
                    }
                }
            }
        }
        protected virtual List<Vector3> GetCells()
        {
            var list = new List<Vector3>();
            return list;
        }
        void DrawBlockPreviews(MySpriteBatch sb, MapBase map, Camera cam)
        {
            var atlastoken = this.Block.GetDefault();
            atlastoken.Atlas.Begin(cam.Effect);
            foreach (var pos in this.ToolDef.Worker.GetPositions(this.Begin, this.EndFinal).Where(map.IsValidBuildSpot))
                this.Block.DrawPreview(sb, map, pos, cam, this.State, this.Material, this.Variation, this.Orientation);
            sb.Flush();
        }
        protected virtual IntVec3 EndFinal => this.End;

        internal override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        {
            base.DrawUI(sb, camera);
            this.Icon.Draw(sb, UIManager.Mouse);
            if (this.Replacing || IsReplacing())
                Icon.Replace.Draw(sb);

            if (!this.Enabled)
                return;
            var txt = string.Join(" x ", this.GetDimensionSize());
            UIManager.DrawStringOutlined(sb, txt, UIManager.Mouse - new Vector2(0, Label.DefaultHeight), Vector2.Zero);
        }
        public virtual IEnumerable<string> GetDimensionSize()
        {
            yield return (Math.Abs(this.End.X - this.Begin.X) + 1).ToString();
            yield return (Math.Abs(this.End.Y - this.Begin.Y) + 1).ToString();
        }

        private static bool IsRemoving()
        {
            return InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey);
        }
        protected bool Replacing;
        private static bool IsReplacing()
        {
            return InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey);
        }

        public IEnumerable<IntVec3> GetPositions(IntVec3 a, IntVec3 b) 
        {
            foreach (var pos in this.ToolDef.Worker.GetPositions(a, b))
                yield return pos;
        }
        public ToolBlockBuild.Args Send(IntVec3 start, IntVec3 end, int orientation)// = 0)
        {
            var a = new ToolBlockBuild.Args(this.ToolDef, start, end, IsRemoving(), IsGodMode(), this.Replacing, orientation);
            this.Callback(a);
            this.Enabled = false;
            this.Replacing = false;
            return a;
        }

        private static bool IsGodMode()
        {
            return false;
        }
        protected override void WriteData(BinaryWriter w)
        {
            w.Write(this.Enabled);
            w.Write(this.Begin);
        }
        protected override void ReadData(BinaryReader r)
        {
            this.Enabled = r.ReadBoolean();
            this.Begin = r.ReadVector3();
        }
        public class Args
        {
            public IntVec3 Begin, End;
            public BuildToolDef ToolDef;
            public bool Removing, Replacing, Cheat;
            public int Orientation;
            public Args(BuildToolDef toolDef, IntVec3 begin, IntVec3 end, bool modkey, bool cheat, bool replacing = false, int orientation = 0)
            {
                this.ToolDef = toolDef;
                this.Begin = begin;
                this.End = end;
                this.Removing = modkey;
                this.Replacing = replacing;
                this.Orientation = orientation;
                this.Cheat = cheat;
            }
            public void Write(BinaryWriter w)
            {
                this.ToolDef.Write(w);
                w.Write(this.Begin);
                w.Write(this.End);
                w.Write(this.Removing);
                w.Write(this.Replacing);
                w.Write(this.Orientation);
                w.Write(this.Cheat);
            }
            public Args(BinaryReader r)
            {
                this.ToolDef = r.ReadDef<BuildToolDef>();
                this.Begin = r.ReadIntVec3();
                this.End = r.ReadIntVec3();
                this.Removing = r.ReadBoolean();
                this.Replacing = r.ReadBoolean();
                this.Orientation = r.ReadInt32();
                this.Cheat = r.ReadBoolean();
            }
        }
    }
}
