using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Simulation;
using Project1.Core.UI.Settings;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Input;
using System;
using System.Windows.Forms;
using System.Xml.Linq;

#nullable enable

namespace Project1.Core.Screens;

sealed class ViewSettings
{
    internal int DrawLevel = MapBase.MaxHeight - 1;
    internal bool HideTerrainAbovePlayer;
    internal int HideTerrainAbovePlayerOffset;
    internal bool MysteriousBlocks;
    internal int FogLevel;
    internal bool DrawZones = true;
}
public sealed class MapViewport(int width, int height, MapBase map, Camera camera)//, Renderer renderer)
{
    static XElement XCameraSettings = GameSettings.XmlNodeSettings.GetOrCreateElement("Camera");

    internal MapBase Map = map;
    internal Camera Camera = camera;
    //internal Renderer Renderer = renderer;
    internal MousePicker Picker = new();
    internal Rectangle Viewport = new(0, 0, width, height);
    internal int Width = width;
    internal int Height = height;
    float FogT;
    internal Entity? Following;
    bool SmoothCentering = (bool?)XCameraSettings.Element(nameof(SmoothCentering)) ?? true;
    internal ViewSettings Settings = new();
    public int LastZTarget;
    public const int FogZOffset = 2, FogFadeLength = 8;
    internal InteractionTarget Mouseover = Controller.Instance.Mouseover.Target;
    internal Vector2 Origin => this.Camera.Coordinates - new Vector2(this.Width, this.Height) / 2 / this.Camera.Zoom;
    internal void Update(int gameSpeed)
    {
        this.UpdateFog(gameSpeed);
        this.Follow();
        if (this.Mouseover is not null)
            this.LastZTarget = (int)this.Mouseover.Global.Z;
    }

    public int FogLevel
        => (int)Math.Max(0, this.LastZTarget - FogZOffset - FogFadeLength);

    internal void SnapToMapCenter()
    {
        var x = this.Map.Size.Blocks / 2;
        var y = x;
        var z = this.Map.GetHeightmapValue(x, y);
        this.Camera.CenterOn(new Vector3(x, y, z), true);
    }
    //internal void ToggleFollow(Entity entity)
    //{
    //    //this.FollowTarget = entity;
    //    this.Camera.ToggleFollowing(entity);
    //}
    internal void ToggleFollow(Entity entity)
    {
        this.Following = this.Following == entity ? null : entity;
    }
    void UpdateFog(int gameSpeed)
    {
        this.FogT = (this.FogT + 0.05f * gameSpeed) % 100;
    }
    public bool CullingCheck(float x, float y, float z, Rectangle sourceBounds, out Rectangle screenBounds)
    {
        //screenBounds = this.Camera.GetScreenBounds(x, y, z, sourceBounds);
        screenBounds = this.GetScreenBounds(x, y, z, sourceBounds);
        return this.Viewport.Intersects(screenBounds);
    }
    public void CenterOn(Vector3 global, bool forceSnap = false)
    {
        //this.Camera.Center = global;
        this.Camera.CenterOn(global, forceSnap);
        this.Settings.DrawLevel = (int)Math.Max(this.Settings.DrawLevel, global.Z + 1);
        //if (!SmoothCentering || forceSnap)
        //{
        //    Coords.Iso(this.Camera, global.X, global.Y, global.Z, out int xx, out int yy);
        //    this.Camera.Coordinates = new Vector2(xx, yy);
        //}
    }
    public void Move(Vector2 coords)
    {
        this.Following = null;

        this.Camera.Center = null;
        this.Camera.Coordinates = coords;
    }
    public void Follow()
    {
        if (this.Following is not Entity tracked)
        {
            this.Camera.SnapToCenter();
            return;
        }
        if (tracked.Map is null)
        {
            this.Following = null;
            return;
        }
        if (tracked.IsIndoors())
            this.Settings.DrawLevel = (int)(tracked.Global.CeilingZ().Z + tracked.Physics.Height - 1);
        else
            this.Settings.DrawLevel = tracked.Map.GetMaxHeight();
        this.Camera.Follow(tracked.Global);
    }
    //internal bool IsDrawable(Vector3 global)
    //{
    //    return global.Z <= this.Renderer.GetMaxDrawLevel(this.Map) + 1;
    //}

    public float GetFarDepth()
    {
        var size = this.Map.GetSizeInChunks() * Chunk.Size;// -1;
        return (int)this.Camera.Rotation switch
        {
            0 => Vector3.Zero.GetDrawDepth(this.Map, this.Camera),
            1 => new Vector3(0, size, 0).GetDrawDepth(this.Map, this.Camera),
            2 => new Vector3(size, size, 0).GetDrawDepth(this.Map, this.Camera),
            3 => new Vector3(size, 0, 0).GetDrawDepth(this.Map, this.Camera),
            _ => 0,
        };
    }
    public float GetNearDepth()
    {
        var size = this.Map.GetSizeInChunks() * Chunk.Size;// -1;
        return (int)this.Camera.Rotation switch
        {
            0 => new Vector3(size, size, 0).GetDrawDepth(this.Map, this.Camera),
            1 => new Vector3(size, 0, 0).GetDrawDepth(this.Map, this.Camera),
            2 => Vector3.Zero.GetDrawDepth(this.Map, this.Camera),
            3 => new Vector3(0, size, 0).GetDrawDepth(this.Map, this.Camera),
            _ => 0,
        };
    }

    internal void SetDrawLevel(int v)
    {
        var old = this.Settings.DrawLevel;
        this.Settings.DrawLevel = v;
        if (InputState.IsKeyDown(Keys.LMenu))
            this.Move(this.Camera.Coordinates - new Vector2(0, Block.BlockHeight * (v - old)));
    }

    int _previousDrawLevel = -1;
    public void SliceOn(int next)
    {
        var current = this.Settings.DrawLevel;
        if (next != current)
        {
            this._previousDrawLevel = current;
            this.Settings.DrawLevel = next;
        }
        else if (this._previousDrawLevel != -1)
            this.Settings.DrawLevel = this._previousDrawLevel;
    }
    internal void AdjustDrawLevel(int p)
    {
        if (!this.Settings.HideTerrainAbovePlayer)
            this.Settings.DrawLevel = Math.Min(MapBase.MaxHeight - 1, Math.Max(0, this.Settings.DrawLevel + p));
        else
            this.Settings.HideTerrainAbovePlayerOffset += p;
    }
    internal bool IsDrawable(Vector3 global)
    {
        return global.Z <= this.GetMaxDrawLevel() + 1;
    }
    public int GetMaxDrawLevel()
    {
        var actor = this.Map.Net.GetPlayer()?.ControllingEntity;
        var value = (this.Settings.HideTerrainAbovePlayer 
            && (actor is not null)) 
            ? (int)actor.Transform.Global.RoundXY().Z + 2 + this.Settings.HideTerrainAbovePlayerOffset 
            : this.Settings.DrawLevel;
        value = Math.Min(MapBase.MaxHeight - 1, Math.Max(0, value));
        return value;
    }

    public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle, int originx, int originy, float scale)
    {
        this.Camera.Iso(x, y, z, out int xx, out int yy);
        var zoom = this.Camera.Zoom;
        var origin = this.Origin;
        var scalezoom = scale * zoom;
        return new Rectangle(
            (int)(zoom * (xx + scale * spriteRectangle.X - origin.X - originx)),
            (int)(zoom * (yy + scale * spriteRectangle.Y - origin.Y - originy)),
            (int)(scalezoom * spriteRectangle.Width),
            (int)(scalezoom * spriteRectangle.Height));
    }
    public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle)
    {
        return this.GetScreenBounds(x, y, z, spriteRectangle, 0, 0);
    }
    public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle, int originx, int originy)
    {
        this.Camera.Iso(x, y, z, out int xx, out int yy);
        var zoom = this.Camera.Zoom;
        var origin = this.Origin;

        return new Rectangle(
            (int)(zoom * (xx + spriteRectangle.X - origin.X - originx)),
            (int)(zoom * (yy + spriteRectangle.Y - origin.Y - originy)),
            (int)(zoom * spriteRectangle.Width),
            (int)(zoom * spriteRectangle.Height));
    }
    public Vector4 GetScreenBoundsVector4(float x, float y, float z, Rectangle spriteRectangle, Vector2 origin, float scale = 1)
    {
        this.Camera.Iso(x, y, z, out float xx, out float yy);
        var loc = this.Origin;
        float xxx = (float)xx + scale * spriteRectangle.X - loc.X - origin.X;
        float yyy = (float)yy + scale * spriteRectangle.Y - loc.Y - origin.Y;
        float w = scale * spriteRectangle.Width;
        float h = scale * spriteRectangle.Height;
        var vector = new Vector4(xxx, yyy, w, h);
        vector *= this.Camera.Zoom;
        return vector;
    }

    public Vector2 GetScreenPositionFloat(Vector3 pos)
    {
        this.Camera.Iso(pos.X, pos.Y, pos.Z, out float xx, out float yy);
        var loc = this.Origin;
        var zoom = this.Camera.Zoom;
        var screenpos = new Vector2(zoom * (xx - loc.X), zoom * (yy - loc.Y));
        return screenpos;
    }

    public Vector2 GetScreenPosition(InteractionTarget t)
    {
        var fx = t.Face.X * .5f;
        var fy = t.Face.Y * .5f;
        var yx = fx + fy;
        var fz = yx == 0 ? (t.Face.Z == 1 ? 1 : 0) : .5f;
        return this.GetScreenPosition(t.Global + new Vector3(fx, fy, fz));
    }

    public Vector2 GetScreenPosition(Vector3 pos)
    {
        this.Camera.Iso(pos.X, pos.Y, pos.Z, out int xx, out int yy);
        //return new Vector2(this.Camera.Zoom * (xx - this.Origin.X), this.Camera.Zoom * (yy - this.Origin.Y));
        return new Vector2(xx - this.Origin.X, yy - this.Origin.Y) * this.Camera.Zoom;
    }
}
