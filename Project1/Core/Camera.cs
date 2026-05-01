using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Simulation;
using Project1.Framework;
using System;

namespace Project1.Core;

public sealed class Camera : ICamera
{
    public float ZoomNext;
    public float Zoom = 2;//1;
    public Vector2 Location;
    public float ZoomMax = 8;// 16;
    public float ZoomMin = 0.125f;
    const float InitialZoom = 2;
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
    public Camera(float x = 0, float y = 0, float z = 0, float zoom = 2, int rotation = 0)
    {
        this.Zoom = zoom;
        this.ZoomNext = zoom;
        this.Rotation = rotation;
        //this.CenterOn(new Vector3(x, y, z));
    }
    //public void CenterOn(Vector3 global, bool forceSnap = false)
    //{
    //    this.Center = global;
    //    if (!SmoothCentering || forceSnap)
    //    {
    //        Coords.Iso(this, global.X, global.Y, global.Z, out int xx, out int yy);
    //        this.Coordinates = new Vector2(xx, yy);
    //    }
    //}
    public void Update(MapBase map)
    {
        this.SmoothZoom(this.ZoomNext);
    }
   
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
    public void Rotate(float x, float y, out int xx, out int yy)
    {
        xx = (int)(x * this.RotCos - y * this.RotSin);
        yy = (int)(x * this.RotSin + y * this.RotCos);
    }
    public void Rotate(float x, float y, out float xx, out float yy)
    {
        xx = (float)(x * this.RotCos - y * this.RotSin);
        yy = (float)(x * this.RotSin + y * this.RotCos);
    }
}
