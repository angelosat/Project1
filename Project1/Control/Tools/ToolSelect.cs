using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    class ToolSelect : ControlTool
    {
        protected IntVec3 Begin, End;
        Action<IntVec3, IntVec3> SelectAction = (a, b) => { };
        public ToolSelect()
        {

        }
        public ToolSelect(TargetArgs target)
        {
            this.Begin = target.Type == TargetType.Position ? this.GetBeginFromTarget(target.Global) : target.Object.Global.ToCell();
            this.End = this.Begin;
        }
        protected virtual IntVec3 GetBeginFromTarget(IntVec3 a)
        {
            return a.Above;
        }
        protected virtual IntVec3 GetEndFromTarget(IntVec3 a)
        {
            return new IntVec3(a.XY, this.Begin.Z);
        }
        protected virtual void Select()
        {
            this.SelectAction(this.Begin, this.End);
            SelectionManager.Select(Client.Instance.Map, this.Begin.GetBoundingBox(this.End));
        }
        public override void Update()
        {
            var cam = Engine.Map.Camera;

            cam.MousePicking(Ingame.DrawServer ? Server.Instance.Map : Client.Instance.Map);
            this.UpdateTarget();

            if (Controller.TargetCell != null)
                this.End = this.GetEndFromTarget(Controller.TargetCell.Global);
        }

        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Target == null)
                return Messages.Default;
            this.Select();
            return Messages.Remove;
        }

        internal override void DrawAfterWorld(MySpriteBatch sb, MapBase map)
        {
            var camera = map.Camera;
            camera.DrawGridBlocks(sb, this.Begin.GetBox(this.End), Color.White);
        }
    }
}
