using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_
{
    class Coords
    {
        static readonly float CosIso = (float)Math.Cos(Math.PI / 4f);
        static readonly float SinIso = (float)Math.Sin(Math.PI / 4f);
        public static Matrix IsoMatrix = new(
                CosIso, -SinIso, 0, 0,
                SinIso, CosIso, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1); 
        
        public static Vector2 GetScreenCoords(Vector3 global, Camera camera, Vector3 unitDimensions)
        {
            float x = global.X, y = global.Y, z = global.Z;
            float w = unitDimensions.X, h = unitDimensions.Y;
            double xr = (x * camera.RotCos - y * camera.RotSin);
            double yr = (x * camera.RotSin + y * camera.RotCos);
            float xx = (int)(w * (xr - yr) / 2);
            float yy = (int)((xr + yr) * h / 2 + z * unitDimensions.Z);

            return new Vector2((int)(xx - camera.Zoom * camera.Location.X), (int)(yy - camera.Zoom * camera.Location.Y));
        }

        public static void Iso(Camera camera, float x, float y, float z, out int xx, out int yy)
        {
            double xr = x * camera.RotCos - y * camera.RotSin;
            double yr = x * camera.RotSin + y * camera.RotCos;
            xx = (int)(Block.Width * (xr - yr) / 2);
            yy = (int)((xr + yr) * Block.Depth / 2 - z * Block.BlockHeight);
        }
        public static void Iso(Camera camera, float x, float y, float z, out float xx, out float yy)
        {
            double xr = x * camera.RotCos - y * camera.RotSin;
            double yr = x * camera.RotSin + y * camera.RotCos;
            xx = (float)(Block.Width * (xr - yr) / 2);
            yy = (float)((xr + yr) * Block.Depth / 2 - z * Block.BlockHeight);
        }
        
        public static void Ortho(int x, int y, out float xx, out float yy)
        {
            Vector2 xy = new Vector2(x, 2 * y);
            xy = Vector2.Transform(xy, IsoMatrix);
            xx = xy.X;
            yy = xy.Y;
        }

        public static Vector2 Rotate(double radians, Vector2 vector)
        {
            float x = vector.X, y = vector.Y;
            return new Vector2((float)(x * Math.Cos(radians) - y * Math.Sin(radians)), (float)(x * Math.Sin(radians) + y * Math.Cos(radians)));
        }
        public static Vector2 Rotate(Camera camera, Vector2 vector)
        {
            float x = vector.X, y = vector.Y;
            return new Vector2((float)(x * camera.RotCos - y * camera.RotSin), (float)(x * camera.RotSin + y * camera.RotCos));
        }
        public static void Rotate(Camera camera, float x, float y, out int xx, out int yy)
        {
            xx = (int)(x * camera.RotCos - y * camera.RotSin);
            yy = (int)(x * camera.RotSin + y * camera.RotCos);
        }
        public static void Rotate(Camera camera, float x, float y, out float xx, out float yy)
        {
            xx = (float)(x * camera.RotCos - y * camera.RotSin);
            yy = (float)(x * camera.RotSin + y * camera.RotCos);
        }
        public static void Rotate(int r, Vector3 global, out Vector3 rotatedGlobal)
        {
            int _r = -r;
            int _Rotation = _r % 4;
            if (_Rotation < 0)
            {
                _Rotation = 4 + _r;
            }

            double RotCos = Math.Cos((Math.PI / 2f) * _Rotation);
            double RotSin = Math.Sin((Math.PI / 2f) * _Rotation);

            RotCos = Math.Round(RotCos + RotCos) / 2f;
            RotSin = Math.Round(RotSin + RotSin) / 2f;

            int xx, yy;
            xx = (int)(global.X * RotCos - global.Y * RotSin);
            yy = (int)(global.X * RotSin + global.Y * RotCos);
            rotatedGlobal = new Vector3(xx, yy, global.Z);
        }
        public static void Rotate(int r, float x, float y, out int xx, out int yy)
        {
            int _r = -r;
            int _Rotation = _r % 4;
            if (_Rotation < 0)
            {
                _Rotation = 4 + _r;
            }

            double RotCos = Math.Cos((Math.PI / 2f) * _Rotation);
            double RotSin = Math.Sin((Math.PI / 2f) * _Rotation);

            RotCos = Math.Round(RotCos + RotCos) / 2f;
            RotSin = Math.Round(RotSin + RotSin) / 2f;

            xx = (int)(x * RotCos - y * RotSin);
            yy = (int)(x * RotSin + y * RotCos);
        }
        
        
        static readonly Matrix MatrixRot0 = new(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        static readonly Matrix MatrixRot3 = new(
           0, -1, 0, 0,
           1, 0, 0, 0,
           0, 0, 1, 0,
           0, 0, 0, 1);
        static readonly Matrix MatrixRot2 = new(
            -1, 0, 0, 0,
            0, -1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        static readonly Matrix MatrixRot1 = new(
            0, 1, 0, 0,
            -1, 0, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);

        public static readonly Matrix[] Orientations = { MatrixRot0, MatrixRot1, MatrixRot2, MatrixRot3 };
        public static Vector3 Rotate(Vector3 vec3, int orientation)
        {
            return Vector3.Transform(vec3, Orientations[orientation]);
        }
        public static IntVec3 Rotate(IntVec3 vec3, int orientation)
        {
            return IntVec3.Transform(vec3, Orientations[orientation]);
        }
    }
}