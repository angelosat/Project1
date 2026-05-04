using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Rendering;
using Project1.Framework;
using System;
using System.ComponentModel.DataAnnotations;

namespace Project1.Core.Graphics;

/// <summary>
/// TODO: optimize
/// </summary>
struct Shadow
{
    public GameObject Parent;
    public Vector3 Global;
    public float Alpha;
    const int ShadowVisibilityRange = 4;

    public Shadow(GameObject parent, Vector3 global)
    {
        this.Parent = parent;
        this.Global = global;
        this.Alpha = Math.Max(0, 1 - (parent.Global.Z - global.Z) / ShadowVisibilityRange);
    }
    public readonly void Draw(MySpriteBatch sb, RenderContext ctx)
    {
        if (ctx.Renderer.IsCompletelyHiddenByFog(this.Global.Z))
            return;
        var camera = ctx.Camera;
        var view = ctx.View;
        //float dn = this.Global.GetDrawDepth(ctx.Map, camera);
        float dn = view.GetDrawDepth(this.Global);
        //Vector2 pos = camera.GetScreenPosition(this.Global).ToFloored();
        var pos = ctx.GetScreenPosition(this.Global).ToFloored();
        Sprite.Shadow.Draw(sb, pos, Color.White * this.Alpha, 0, Sprite.Shadow.OriginGround, camera.Zoom*Parent.Body.Scale, SpriteEffects.None, dn);
    }
}
