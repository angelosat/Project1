using Microsoft.Xna.Framework;
using Project1.Core.Graphics;
using Project1.Core.Input;
using System;
using System.Windows.Forms;

namespace Project1.Core.Construction.Tools;

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
    public override ControlTool.Messages MouseLeftUp(HandledMouseEventArgs e)
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
    
    protected override void DrawGrid(MySpriteBatch sb, RenderContext ctx, Color color)
    {
        ctx.Renderer.DrawBlockMouseover(sb, this.Begin, this.Valid ? Color.Lime : Color.Red);
    }
}
