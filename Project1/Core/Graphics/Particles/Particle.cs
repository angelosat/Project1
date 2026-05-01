using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Blocks;
using Project1.Core.Helpers;
using Project1.Framework;
using System;

namespace Project1.Core.Graphics.Particles;

public sealed class Particle
{
    public Texture2D Texture = Block.ParticlePixel.Atlas.Texture;
    public Rectangle SourceRectangle = Block.ParticlePixel.Rectangle;

    public Vector3 Offset, Velocity;
    public float Lifetime, LifetimeMax;
    public Func<Particle, Color> ColorFunc = p => Color.White;
    public Func<Particle, float> ScaleFunc = p => 2;
    public Func<Particle, float> AlphaFunc = p => p.LifePercentage;

    public float LifePercentage { get { return this.Lifetime / this.LifetimeMax; } }

    public Particle(Vector3 startOffset, Vector3 startVelocity, float life)
    {
        this.Offset = startOffset;
        this.Velocity = startVelocity;
        this.Lifetime = this.LifetimeMax = life;
    }

    public void Update()
    {
        this.Lifetime--;
    }

    public void Draw(MySpriteBatch sb, RenderContext ctx, Vector3 global)
    {
        var renderer = ctx.Renderer;
        var map = ctx.Map;
        var camera = ctx.Camera;
        var view = ctx.View;
        var transformedGlobal = this.Offset + global;
        if (transformedGlobal.Z > renderer.MaxDrawZ + 1)
            return;
        var rounded = transformedGlobal.ToRounded();
        map.GetLight(transformedGlobal, out byte skylight, out byte blocklight);
        var skyColor = map.GetAmbientColor() * ((skylight + 1) / 16f);
        skyColor.A = 255;
        var blockColorVector = Vector4.Lerp(new Vector4(0, 0, 0, 1), Vector4.One, (blocklight) / 15f);

        var screenpos = view.GetScreenPositionFloat(transformedGlobal);
        var alpha = this.AlphaFunc(this);
        var scale = this.ScaleFunc(this);
        var color = this.ColorFunc(this);
        //var depth = transformedGlobal.GetDrawDepth(map, camera);
        var depth = view.GetDrawDepth(transformedGlobal);
        var finalscale = new Vector2(camera.Zoom) * scale;
        var finalcolor = color * alpha;
        var origin = new Vector2(this.SourceRectangle.Width, this.SourceRectangle.Height) / 2f;
        sb.Draw(
            this.Texture, screenpos, this.SourceRectangle,
            0, origin, finalscale,
            skyColor, blockColorVector,
            finalcolor, Color.Transparent,
            SpriteEffects.None, depth);
    }
}
