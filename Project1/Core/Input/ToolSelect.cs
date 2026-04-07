using System;
using Microsoft.Xna.Framework;
using Project1.Core.Screens;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.UI.Hud;
using Project1.Framework;
using Project1.Core.Graphics;
using Project1.Core.Networking;
using Project1.Framework.Helpers;

namespace Project1.Core.Input
{
    class ToolSelect : ControlTool
    {
        protected IntVec3 Begin, End;
        Action<IntVec3, IntVec3> SelectAction = (a, b) => { };
        public ToolSelect()
        {

        }
        public ToolSelect(InteractionTarget target)
        {
            this.Begin = target.Type == TargetType.Cell ? this.GetBeginFromTarget(target.Global) : target.Object.Global.ToCell();
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
            SelectionManager.Select(this.Map, this.Begin.GetBoundingBox(this.End));
        }
        public override void Update()
        {
            //var cam = Engine.Map.Camera;
            var cam = this.Map.Camera;
            //cam.MousePicking(Ingame.DrawServer ? Server.Instance.Map : Client.Instance.Map);
            cam.MousePicking(this.Map);
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
