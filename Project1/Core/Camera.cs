using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Input;
using System;
using System.Windows.Forms;

namespace Project1.Core;

public sealed class Camera : ICamera, IInputEventHandler
{
    //Entity Following;
    public float ZoomNext;
    public float Zoom = 2;//1;
    public Vector2 Location;
    public float ZoomMax = 8;// 16;
    public float ZoomMin = 0.125f;
    const float InitialZoom = 2;
    //public Rectangle ViewPort;
    Vector2 _Coordinates;
    public int Width, Height;
    public static bool SmoothCentering;

    public Vector2 Coordinates
    {
        get => this._Coordinates;
        set
        {
            this._Coordinates = value;
            this.Location = this.Coordinates - new Vector2((int)((this.Width / 2) / this.Zoom), (int)((this.Height / 2) / this.Zoom));
        }
    }
    public Vector3? Center = Vector3.Zero;

    public double RotCos { get; private set; }
    public double RotSin { get; private set; }
    public int RotationReverse { get; private set; }

    public double Rotation
    {
        get => field;
        set
        {
            double oldRot = field;
            field = value % 4;

            if (field < 0)
                field = 4 + value;

            this.RotationReverse = -(int)field;
            if (this.RotationReverse < 0)
                this.RotationReverse += 4;

            this.RotCos = Math.Cos((Math.PI / 2f) * field);
            this.RotSin = Math.Sin((Math.PI / 2f) * field);

            this.RotCos = Math.Round(this.RotCos + this.RotCos) / 2f;
            this.RotSin = Math.Round(this.RotSin + this.RotSin) / 2f;


            if (field != oldRot)
                this.OnRotationChanged();
        }
    }
    public Camera(/*int width, int height, */float x = 0, float y = 0, float z = 0, float zoom = 2, int rotation = 0)
    {
        //this.Width = width;
        //this.Height = height;
        //this.ViewPort = new Rectangle(0, 0, this.Width, this.Height);
        this.Zoom = zoom;
        this.ZoomNext = zoom;
        this.Rotation = rotation;
        this.CenterOn(new Vector3(x, y, z));
        //Game1.Instance.graphics.DeviceReset += this.gfx_DeviceReset;
        //this.OnDeviceLost();
    }
    public void CenterOn(Vector3 global, bool forceSnap = false)
    {
        this.Center = global;
        //this.Renderer.DrawLevel = (int)Math.Max(this.Renderer.DrawLevel, global.Z + 1);
        if (!SmoothCentering || forceSnap)
        {
            Coords.Iso(this, global.X, global.Y, global.Z, out int xx, out int yy);
            this.Coordinates = new Vector2(xx, yy);
        }
    }
    public void Update(MapBase map)
    {
        //this.Follow();
        this.SmoothZoom(this.ZoomNext);
    }
    //public void Follow()
    //{
    //    if (this.Following is null)
    //    {
    //        if (this.Center.HasValue)
    //            this.Follow(this.Center.Value);
    //        return;
    //    }
    //    if (this.Following.Map is null)
    //    {
    //        this.Following = null;
    //        return;
    //    }
    //    if (this.Following.IsIndoors())
    //        this.DrawLevel = (int)(this.Following.Global.CeilingZ().Z + this.Following.Physics.Height - 1);
    //    else
    //        this.DrawLevel = this.Following.Map.GetMaxHeight();
    //    this.Follow(this.Following.Global);
    //}
   
    public void SmoothZoom(float next)
    {
        float diff = next - this.Zoom;
        var zoomSpeed = 0.1f;
        var n = zoomSpeed * diff;
        if (Math.Abs(n) < 0.001f)
            this.SetZoom(next);
        else
            this.SetZoom(this.Zoom + n);
    }
    void SetZoom(float value)
    {
        this.Zoom = value;
        var offset = new Vector2(this.Width / 2, this.Height / 2);
        offset /= this.Zoom;
        this.Location = this.Coordinates - offset;
    }
    internal void SnapToCenter()
    {
        if (!this.Center.HasValue)
            return;
        this.Follow(this.Center.Value);
    }
    public void Follow(Vector3 global)
    {
        this.Center = global;
        Coords.Iso(this, global.X, global.Y, global.Z, out float xx, out float yy);

        Vector2
            currentLoc = this.Coordinates,
            nextLoc = new(xx, yy),
            diff = nextLoc - currentLoc;

        diff *= 100;
        diff = diff.ToRounded();
        diff /= 100;

        var nextCoords = currentLoc + 0.05f * diff;

        // TODO: find a way to make it smooth without seaming between sprites

        /// uncomment this to make camera movement rigid instead of smooth
        nextCoords = nextCoords.ToRounded(); // must round to prevent seaming between blocks when moving camera
        ///

        this.Coordinates = nextCoords;
    }
    void OnRotationChanged()
    {
        //Ingame.MainViewportMap.OnCameraRotated(this);
        //SelectionManager.Instance.OnCameraRotated(this);
    }
    public void RotateClockwise()
    {
        this.Rotation++;
    }
    public void RotateCounterClockwise()
    {
        this.Rotation--;
    }
    public void RotationReset()
    {
        this.Rotation = 0;
    }
    //internal void ToggleFollowing(Entity entity)
    //{
    //    this.Following = this.Following == entity ? null : entity;
    //}
    
    internal float GetDrawDepth(GameObject o)
    {
        return o.Global.GetDrawDepth(o.Map, this);
    }
    internal float GetDrawDepth(MapBase map, Vector3 global)
    {
        return global.GetDrawDepth(map, this);
    }
    internal int GetDrawDepthSimple(IntVec3 global)
    {
        Coords.Rotate(this, global.X, global.Y, out int rx, out int ry);
        return rx + ry + global.Z;
    }
    
    public void GetEverything(MapBase map, Vector3 global, Rectangle spriteRect, out float depth, out Rectangle screenBounds, out Vector2 screenLoc)
    {
        depth = global.GetDrawDepth(map, this);
        screenBounds = this.GetScreenBounds(global, spriteRect);
        screenLoc = new Vector2(screenBounds.X, screenBounds.Y);
    }
   public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle, int originx, int originy)
    {
        Coords.Iso(this, x, y, z, out int xx, out int yy);
        return new Rectangle(
            (int)(this.Zoom * (xx + spriteRectangle.X - this.Location.X - originx)),
            (int)(this.Zoom * (yy + spriteRectangle.Y - this.Location.Y - originy)),
            (int)(this.Zoom * spriteRectangle.Width),
            (int)(this.Zoom * spriteRectangle.Height));
    }
    public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle, int originx, int originy, float scale)
    {
        Coords.Iso(this, x, y, z, out int xx, out int yy);
        var scalezoom = scale * this.Zoom;
        return new Rectangle(
            (int)(this.Zoom * (xx + scale * spriteRectangle.X - this.Location.X - originx)),
            (int)(this.Zoom * (yy + scale * spriteRectangle.Y - this.Location.Y - originy)),
            (int)(scalezoom * spriteRectangle.Width),
            (int)(scalezoom * spriteRectangle.Height));
    }
    
    public Vector4 GetScreenBoundsVector4(float x, float y, float z, Rectangle spriteRectangle, Vector2 origin, float scale = 1)
    {
        Coords.Iso(this, x, y, z, out float xx, out float yy);
        var loc = this.Location;
        float xxx = (float)xx + scale * spriteRectangle.X - loc.X - origin.X;
        float yyy = (float)yy + scale * spriteRectangle.Y - loc.Y - origin.Y;
        float w = scale * spriteRectangle.Width;
        float h = scale * spriteRectangle.Height;
        var vector = new Vector4(xxx, yyy, w, h);
        vector *= this.Zoom;
        return vector;
    }
    public Vector4 GetScreenBoundsVector4NoOffset(float x, float y, float z, Rectangle spriteRectangle, Vector2 origin)
    {
        Coords.Iso(this, x, y, z, out float xx, out float yy);
        float xxx = (float)(xx + spriteRectangle.X - origin.X);
        float yyy = (float)(yy + spriteRectangle.Y - origin.Y);
        float w = spriteRectangle.Width;
        float h = spriteRectangle.Height;
        var vector = new Vector4(xxx, yyy, w, h);
        return vector;
    }
    public Vector4 GetScreenBoundsVector4NoOffset(Vector3 pos, Rectangle spriteRectangle, Vector2 origin)
    {
        Coords.Iso(this, pos.X, pos.Y, pos.Z, out float xx, out float yy);
        float xxx = (float)(xx + spriteRectangle.X - origin.X);
        float yyy = (float)(yy + spriteRectangle.Y - origin.Y);
        float w = spriteRectangle.Width;
        float h = spriteRectangle.Height;
        var vector = new Vector4(xxx, yyy, w, h);
        return vector;
    }
    //public Vector2 GetScreenPosition(InteractionTarget t)
    //{
    //    var fx = t.Face.X * .5f;
    //    var fy = t.Face.Y * .5f;
    //    var yx = fx + fy;
    //    var fz = yx == 0 ? (t.Face.Z == 1 ? 1 : 0) : .5f;
    //    return this.GetScreenPosition(t.Global + new Vector3(fx, fy, fz));
    //}
    public Vector2 GetScreenPosition(Vector3 pos)
    {
        Coords.Iso(this, pos.X, pos.Y, pos.Z, out int xx, out int yy);
        return new Vector2(this.Zoom * (xx - this.Location.X), this.Zoom * (yy - this.Location.Y));
    }
    public Vector2 GetScreenPositionFloat(Vector3 pos)
    {
        Coords.Iso(this, pos.X, pos.Y, pos.Z, out float xx, out float yy);
        var loc = this.Location;
        var screenpos = new Vector2(this.Zoom * (xx - loc.X), this.Zoom * (yy - loc.Y));
        return screenpos;
    }
    public Rectangle GetScreenBounds(Vector3 global, Rectangle spriteRectangle)
    {
        return this.GetScreenBounds(global.X, global.Y, global.Z, spriteRectangle);
    }
    public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle)
    {
        return this.GetScreenBounds(x, y, z, spriteRectangle, 0, 0);
    }
    
    public void ZoomIncrease()
    {
        this.ZoomNext *= 2;
        this.ZoomNext = MathHelper.Clamp(this.ZoomNext, this.ZoomMin, this.ZoomMax);

    }
    public void ZoomDecrease()
    {
        this.ZoomNext /= 2;
        this.ZoomNext = MathHelper.Clamp(this.ZoomNext, this.ZoomMin, this.ZoomMax);
    }
    public void ZoomReset()
    {
        this.ZoomNext = InitialZoom;
    }

    //public void MousePicking(MapBase map, bool ignoreEntities = false)
    //{
    //    var visibleChunks = map.GetActiveChunks().Values.Where(ch => this.ViewPort.Intersects(ch.GetScreenBounds(this)));
    //    if (!(ignoreEntities || Controller.IsBlockTargeting()))
    //        foreach (var chunk in visibleChunks)
    //            chunk.HitTestEntities(this);

    //    /// uncomment this to prefer targetting entities even when they are behind blocks
    //    //if (Controller.Instance.MouseoverNext.Object is not null)
    //    //    return;

    //    if (!BlockTargeting)
    //        return;

    //    var controller = Controller.Instance;
    //    var hidewalls = Engine.HideWalls;
    //    var actor = map.Net.GetPlayer()?.ControllingEntity;
    //    var playerExists = actor != null;
    //    var playerGlobal = playerExists ? actor.Global : default;
    //    var radius = .01f * this.Zoom * this.Zoom; //occlusion radius
    //    var found = false;
    //    var foundDepth = float.MinValue;
    //    var foundGlobal = Vector3.Zero;
    //    var foundMouse = Vector2.Zero;
    //    Block foundBlock;
    //    var foundRect = Rectangle.Empty;
    //    var camx = this.Coordinates.X - (this.Width / 2f) / this.Zoom;
    //    var camy = this.Coordinates.Y - (this.Height / 2f) / this.Zoom;
    //    var mouse = UIManager.Mouse;
    //    var mousex = (int)mouse.X;
    //    var mousey = (int)mouse.Y;
    //    var behind = InputState.IsKeyDown(Keys.Menu);

    //    var rectw = (int)(Block.Width * this.Zoom);
    //    var recth = (int)(Block.Height * this.Zoom);
    //    foreach (var chunk in visibleChunks)
    //    {
    //        var chunkBounds = chunk.GetScreenBounds(this);
    //        if (!chunkBounds.Contains(mousex, mousey))
    //            continue;

    //        Coords.Iso(this, chunk.X * Chunk.Size, chunk.Y * Chunk.Size, 0, out float chunkx, out float chunky);
    //        chunkx -= camx;
    //        chunky -= camy;

    //        var foglvl = this.GetFogLevel();
    //        for (int j = this.MaxDrawZ; j >= foglvl; j--)
    //        {
    //            var slice = chunk.Slices[j];
    //            if (slice is null)
    //                continue;

    //            /// removing this check because it screws up mousepicking when slices are invalidated by blocks changing (like actors trampling grass)
    //            //if (!slice.Valid)
    //            //    continue;
    //            if (slice.Canvas is null)
    //                continue;


    //            var arrays = slice.Canvas.GetMouseoverableMeshes();


    //            //if (j == this.MaxDrawZ)
    //            //    arrays.Add(slice.Unknown.vertices);
    //            if (j == this.MaxDrawZ)
    //            {
    //                // i've consolidated mysterious blocks into the "cover" canvas, and removed the "unknown" spritebatch from the slice structure
    //                //if(this.MysteriousBlocks)
    //                //    arrays = arrays.Append(slice.Unknown.vertices);
    //                //else
    //                arrays = arrays.Concat(slice.Cover.GetMouseoverableMeshes());
    //            }

    //            // HACK
    //            //if(map.Town.DesignationManager.Renderers[DesignationDefOf.Construct].Slices.TryGetValue(j, out var constructionDesignationMesh))
    //            //    arrays = arrays.Append(constructionDesignationMesh.vertices);


    //            foreach (var array in arrays)
    //            {
    //                var count = array.Length;
    //                for (int i = count - 4; i >= 0; i -= 4)
    //                {
    //                    if (!this.EarlyOutMousePicking(array, i, mousex, mousey, chunkx, chunky, rectw, recth, out int rectx, out int recty, out Vector3 global))
    //                        continue;


    //                    //var block = chunk.GetBlockFromGlobal(global.X, global.Y, global.Z);
    //                    var block = map.GetCell(global).Block;

    //                    if (!block.IsTargetable(global))
    //                        continue;

    //                    if (hidewalls)
    //                    {
    //                        if (playerExists)
    //                        {
    //                            if (global.Z >= playerGlobal.Z)
    //                            {
    //                                if (global.X + global.Y > playerGlobal.X + playerGlobal.Y)
    //                                {
    //                                    if (block.Opaque)
    //                                    {
    //                                        //distance between mouse and center of screen normalized between -1,1
    //                                        var dx = mousex - this.Width / 2f;
    //                                        var dy = mousey - this.Height / 2f;
    //                                        var d = new Vector2(dx, dy);
    //                                        d.Y /= this.Width / (float)this.Height;
    //                                        d /= new Vector2(this.Width / 2f, this.Height / 2f);
    //                                        var l = d.LengthSquared();
    //                                        if (l < radius)
    //                                            continue;
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }

    //                    var xx = (int)((mousex - rectx) / this.Zoom);
    //                    var yy = (int)((mousey - recty) / this.Zoom);
    //                    if (!block.MouseMap.HitTestEarly(xx, yy))
    //                        continue;

    //                    Coords.Rotate(this, global.X, global.Y, out int rx, out int ry);
    //                    var currentDepth = rx + ry + global.Z;

    //                    if (currentDepth > foundDepth)
    //                    {
    //                        foundDepth = currentDepth;
    //                        foundGlobal = global;
    //                        foundMouse = mouse;
    //                        foundRect = new Rectangle(rectx, recty, rectw, recth);
    //                        foundBlock = block;
    //                        found = true;
    //                    }
    //                    //}
    //                }
    //            }
    //        }

    //    }
    //    if (found)
    //    {
    //        // create mouseover anyway even if air in case of undiscovered area? or check drawunknownblocks?
    //        this.CreateMouseover(map, foundGlobal, foundRect, foundMouse, behind);
    //    }
    //}
    public void HandleKeyDown(KeyEventArgs e)
    {
    }

    public void HandleKeyPress(KeyPressEventArgs e)
    {
    }

    public void HandleKeyUp(KeyEventArgs e)
    {
    }

    public void HandleLButtonDoubleClick(HandledMouseEventArgs e)
    {
    }

    public void HandleLButtonDown(HandledMouseEventArgs e)
    {
    }

    public void HandleLButtonUp(HandledMouseEventArgs e)
    {
    }

    public void HandleMiddleDown(HandledMouseEventArgs e)
    {
    }

    public void HandleMiddleUp(HandledMouseEventArgs e)
    {
    }

    public void HandleMouseMove(HandledMouseEventArgs e)
    {
    }

    public void HandleMouseWheel(HandledMouseEventArgs e)
    {
    }

    public void HandleRButtonDown(HandledMouseEventArgs e)
    {
    }

    public void HandleRButtonUp(HandledMouseEventArgs e)
    {
    }

    public void Iso(float x, float y, float z, out float xx, out float yy)
    {
        double xr = x * this.RotCos - y * this.RotSin;
        double yr = x * this.RotSin + y * this.RotCos;
        xx = (float)(Block.Width * (xr - yr) / 2);
        yy = (float)((xr + yr) * Block.Depth / 2 - z * Block.BlockHeight);
    }
    public void Iso(float x, float y, float z, out int xx, out int yy)
    {
        double xr = x * this.RotCos - y * this.RotSin;
        double yr = x * this.RotSin + y * this.RotCos;
        xx = (int)(Block.Width * (xr - yr) / 2);
        yy = (int)((xr + yr) * Block.Depth / 2 - z * Block.BlockHeight);
    }
}
