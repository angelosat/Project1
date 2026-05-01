using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using System;

namespace Project1.Core;

public record struct Viewport(int Width, int Height);
public struct RenderContext
{
    public MapBase Map;
    public Camera Camera;
    public Rectangle Viewport;
    public Renderer Renderer;
    public MapView View;

    //public Vector2 Pos;
    //public Vector2 ScreenCenter;
    public Vector2 Origin;

    //float RotSin;
    //float RotCos;
    //float Zoom;

    float Zoom => this.Camera.Zoom;

    int DrawLevel;

    //public void CenterOn(Vector3 global, bool forceSnap = false)
    //{
    //    //this.Camera.Center = global;
    //    this.Camera.CenterOn(global, forceSnap);
    //    this.DrawLevel = (int)Math.Max(this.DrawLevel, global.Z + 1);
    //    //if (!SmoothCentering || forceSnap)
    //    //{
    //    //    Coords.Iso(this.Camera, global.X, global.Y, global.Z, out int xx, out int yy);
    //    //    this.Camera.Coordinates = new Vector2(xx, yy);
    //    //}
    //}

    public Vector4 GetScreenBoundsVector4(float x, float y, float z, Rectangle spriteRectangle, Vector2 origin, float scale = 1)
    {
        this.Iso(x, y, z, out float xx, out float yy);
        var loc = this.Origin;
        float xxx = (float)xx + scale * spriteRectangle.X - loc.X - origin.X;
        float yyy = (float)yy + scale * spriteRectangle.Y - loc.Y - origin.Y;
        float w = scale * spriteRectangle.Width;
        float h = scale * spriteRectangle.Height;
        var vector = new Vector4(xxx, yyy, w, h);
        vector *= this.Camera.Zoom;
        return vector;
    }
    //public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle, int originx, int originy, float scale)
    //{
    //    this.Iso(x, y, z, out int xx, out int yy);
    //    var zoom = this.Camera.Zoom;
    //    var origin = this.Origin;
    //    var scalezoom = scale * zoom;
    //    return new Rectangle(
    //        (int)(zoom * (xx + scale * spriteRectangle.X - origin.X - originx)),
    //        (int)(zoom * (yy + scale * spriteRectangle.Y - origin.Y - originy)),
    //        (int)(scalezoom * spriteRectangle.Width),
    //        (int)(scalezoom * spriteRectangle.Height));
    //}
    public void Iso(float x, float y, float z, out int xx, out int yy)
    {
        double xr = x * this.Camera.RotCos - y * this.Camera.RotSin;
        double yr = x * this.Camera.RotSin + y * this.Camera.RotCos;
        xx = (int)(Block.Width * (xr - yr) / 2);
        yy = (int)((xr + yr) * Block.Depth / 2 - z * Block.BlockHeight);
    }
    public void Iso(float x, float y, float z, out float xx, out float yy)
    {
        double xr = x * this.Camera.RotCos - y * this.Camera.RotSin;
        double yr = x * this.Camera.RotSin + y * this.Camera.RotCos;
        xx = (float)(Block.Width * (xr - yr) / 2);
        yy = (float)((xr + yr) * Block.Depth / 2 - z * Block.BlockHeight);
    }
    public Vector2 GetScreenPosition(Vector3 pos)
    {
        this.Iso(pos.X, pos.Y, pos.Z, out int xx, out int yy);
        return new Vector2(this.Zoom * (xx - this.Origin.X), this.Zoom * (yy - this.Origin.Y));
    }

}
