using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Graphics;
using System;
using System.Windows.Forms;

namespace Project1.Core.Input;

class EmptyTool : ControlTool
{
    public Func<InteractionTarget, ControlTool.Messages>
        LeftClick = (target) => { return ControlTool.Messages.Default; },
        RightClick = (target) => { return ControlTool.Messages.Remove; };
    public Func<KeyEventArgs, ControlTool.Messages>
        KeyUp = (e) => { return ControlTool.Messages.Default; };
    public Action<SpriteBatch, Renderer> DrawAction = (sb, cam) => { };
    public Action<MySpriteBatch, Renderer> DrawActionMy = (sb, cam) => { };

    public override ControlTool.Messages MouseLeftUp(HandledMouseEventArgs e)
    {
        if (e.Handled)
            return Messages.Default;
        if(this.Target == null)
            return Messages.Default;
        if (Controller.Instance.Mouseover.Target.Global != Target.Global)
            return Messages.Default;
        return this.Target != null ? LeftClick(this.Target) : Messages.Default;
    }
    
    public override ControlTool.Messages MouseRightDown(HandledMouseEventArgs e)
    {
        return RightClick(this.Target);
    }
    
    public override void HandleKeyUp(KeyEventArgs e)
    {
        KeyUp(e);
    }

    internal override void DrawBeforeWorld(MySpriteBatch sb, RenderContext ctx)
    {
        base.DrawBeforeWorld(sb, ctx);
        DrawActionMy(sb, ctx.Renderer);
    }
}
