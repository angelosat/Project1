using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class ToolBuildSingle : ToolBlockBuild
    {
        public override string Status => "Select location";

        public ToolBuildSingle()
        {

        }
        public ToolBuildSingle(Action<Args> callback)
            : base(callback)
        {

        }
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.Enabled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            this.Send(this.Begin, this.Begin, this.Orientation);
            this.Enabled = false;
            return Messages.Default;
        }
        
        public override void Update()
        {
            base.Update();
        }
        
        protected override void DrawGrid(MySpriteBatch sb, MapBase map, Camera cam, Color color)
        {
            cam.DrawBlockMouseover(sb, map, this.Begin, this.Valid ? Color.Lime : Color.Red);
        }
    }
}
