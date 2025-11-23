using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static class VectorHelper
    {
        static readonly IEnumerable<IntVec3> AdjacentRadial = IntVec3.Zero.GetRadial(2);
        static readonly IEnumerable<IntVec3> AdjacentRadialLarge = IntVec3.Zero.GetRadial(Chunk.Size);
        static public IEnumerable<IntVec3> GetRadial(this IntVec3 center, int radius)
        {
            var r = Vector3.One * radius;
            var box = new BoundingBox((Vector3)center - r, (Vector3)center + r).GetBoxIntVec3();
            box.Sort((a, b) =>
            {
                float aa = a.LengthSquared();
                float bb = b.LengthSquared();
                if (aa < bb)
                    return -1;
                else if (aa == bb)
                    return 0;
                else
                    return 1;
            });
            foreach (var v in box)
                yield return v;
        }
       
        static public IEnumerable<Vector3> GetRadial(this IntVec3 center)
        {
            foreach (var n in AdjacentRadial)
                yield return center + n;
        }
        static public IEnumerable<IntVec3> GetRadialLarge(this IntVec3 center)
        {
            foreach (var n in AdjacentRadialLarge)
                yield return center + n;
        }
        static public void GetMinMaxVector3(IntVec3 vec1, IntVec3 vec2, out IntVec3 min, out IntVec3 max)
        {
            var xmin = Math.Min(vec1.X, vec2.X);
            var ymin = Math.Min(vec1.Y, vec2.Y);
            var zmin = Math.Min(vec1.Z, vec2.Z);
            var xmax = vec1.X + vec2.X - xmin;
            var ymax = vec1.Y + vec2.Y - ymin;
            var zmax = vec1.Z + vec2.Z - zmin;
            min = new IntVec3(xmin, ymin, zmin);
            max = new IntVec3(xmax, ymax, zmax);
        }
        static public void GetMinMaxVector3(Vector3 vec1, Vector3 vec2, out Vector3 min, out Vector3 max)
        {
            var xmin = Math.Min(vec1.X, vec2.X);
            var ymin = Math.Min(vec1.Y, vec2.Y);
            var zmin = Math.Min(vec1.Z, vec2.Z);
            var xmax = vec1.X + vec2.X - xmin;
            var ymax = vec1.Y + vec2.Y - ymin;
            var zmax = vec1.Z + vec2.Z - zmin;
            min = new Vector3(xmin, ymin, zmin);
            max = new Vector3(xmax, ymax, zmax);
        }
        static public void GetMinMaxVector2(Vector2 vec1, Vector2 vec2, out Vector2 min, out Vector2 max)
        {
            var xmin = Math.Min(vec1.X, vec2.X);
            var ymin = Math.Min(vec1.Y, vec2.Y);
            var xmax = vec1.X + vec2.X - xmin;
            var ymax = vec1.Y + vec2.Y - ymin;
            min = new Vector2(xmin, ymin);
            max = new Vector2(xmax, ymax);
        }

        static public readonly Vector3[] Adjacent = new Vector3[]{
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1)
        };
        
        static public readonly IntVec3[] AdjacentXY = new IntVec3[]{
            new IntVec3(1, 0, 0),
            new IntVec3(-1, 0, 0),
            new IntVec3(0, 1, 0),
            new IntVec3(0, -1, 0)
        };
       
        static public readonly IntVec3[] Column3 = new IntVec3[]{
            new IntVec3(0, 0, 1),
            new IntVec3(0, 0, 0),
            new IntVec3(0, 0, -1)
        };
        static public Vector3[] GetAdjacent(this Vector3 center)
        {
            var array = new Vector3[6];
            for (int i = 0; i < Adjacent.Length; i++)
            {
                array[i] = center + Adjacent[i];
            }
            return array;
        }
        static public IEnumerable<Vector3> GetAdjacentLazy(this Vector3 global)
        {
            yield return global + new Vector3(1, 0, 0);
            yield return global - new Vector3(1, 0, 0);
            yield return global + new Vector3(0, 1, 0);
            yield return global - new Vector3(0, 1, 0);
            yield return global + new Vector3(0, 0, 1);
            yield return global - new Vector3(0, 0, 1);
        }
        

        public static Vector2 ToFloored(this Vector2 vector)
        {
            vector.X = (int)Math.Floor(vector.X);
            vector.Y = (int)Math.Floor(vector.Y);
            return vector;
        }
        public static Vector3 Floor(this Vector3 vector)
        {
            vector.X = (int)Math.Floor(vector.X);
            vector.Y = (int)Math.Floor(vector.Y);
            vector.Z = (int)Math.Floor(vector.Z);
            return vector;
        }
        public static Vector2 ToRounded(this Vector2 vector)
        {
            vector.X = (int)Math.Round(vector.X);
            vector.Y = (int)Math.Round(vector.Y);
            return vector;
        }
        public static Vector2 Round(this Vector2 vector, int decimalpoints)
        {
            vector.X = (float)Math.Round(vector.X, decimalpoints);
            vector.Y = (float)Math.Round(vector.Y, decimalpoints);
            return vector;
        }
        public static Vector3 RoundXY(this Vector3 vector)
        {
            vector.X = (int)Math.Round(vector.X);
            vector.Y = (int)Math.Round(vector.Y);
            vector.Z = (int)vector.Z;
            return vector;
        }
        public static Vector3 FloorXY(this Vector3 vector)
        {
            vector.X = (int)Math.Floor(vector.X);
            vector.Y = (int)Math.Floor(vector.Y);
            vector.Z = (int)vector.Z;
            return vector;
        }
        public static Vector3 ToRounded(this Vector3 vector)
        {
            vector.X = (int)Math.Round(vector.X);
            vector.Y = (int)Math.Round(vector.Y);
            vector.Z = (int)Math.Round(vector.Z);
            return vector;
        }
        public static IntVec3 ToCell(this Vector3 vector)
        {
            vector.X = (int)Math.Round(vector.X);
            vector.Y = (int)Math.Round(vector.Y);
            vector.Z = (int)Math.Floor(vector.Z);
            return vector;
        }
        public static Vector3 Round(this Vector3 vector, int decimalPoints)
        {
            vector.X = (float)Math.Round(vector.X, decimalPoints);
            vector.Y = (float)Math.Round(vector.Y, decimalPoints);
            vector.Z = (float)Math.Round(vector.Z, decimalPoints);
            return vector;
        }
        public static Vector3 Normalized(this Vector3 vector)
        {
            vector.Normalize();
            return vector;
        }
        public static Vector3 DirectionTo(this Vector3 vector, Vector3 target)
        {
            var dir = (target - vector).Normalized();
            return dir;
        }
        public static Vector3 Rotate(this Vector3 pos, double quarters)
        {
            double rotCos = Math.Cos((Math.PI / 2f) * quarters);
            double rotSin = Math.Sin((Math.PI / 2f) * quarters);

            rotCos = Math.Round(rotCos + rotCos) / 2f;
            rotSin = Math.Round(rotSin + rotSin) / 2f;
            return new Vector3((float)(pos.X * rotCos - pos.Y * rotSin), (float)(pos.X * rotSin + pos.Y * rotCos), pos.Z);
        }
        public static Vector3 Rotate(this Vector3 pos, Camera camera)
        {
            return new Vector3((
                float)(pos.X * camera.RotCos - pos.Y * camera.RotSin),
                (float)(pos.X * camera.RotSin + pos.Y * camera.RotCos),
                pos.Z);
        }
        public static Vector3 Rotate(this IntVec3 pos, Camera camera)
        {
            return new IntVec3(
                (int)(pos.X * camera.RotCos - pos.Y * camera.RotSin),
                (int)(pos.X * camera.RotSin + pos.Y * camera.RotCos),
                pos.Z);
        }
        public static Vector2 Rotate(this Vector2 pos, Camera camera)
        {
            return new Vector2((float)(pos.X * camera.RotCos - pos.Y * camera.RotSin), (float)(pos.X * camera.RotSin + pos.Y * camera.RotCos));
        }

        static public Vector3[] GetNeighborsSameZ(this Vector3 global)
        {
            Vector3[] neighbors = new Vector3[4];
            neighbors[0] = global + new Vector3(1, 0, 0);
            neighbors[1] = global - new Vector3(1, 0, 0);
            neighbors[2] = global + new Vector3(0, 1, 0);
            neighbors[3] = global - new Vector3(0, 1, 0);
            return neighbors;
        }
        static public IEnumerable<Vector3> GetNeighbors(this Vector3 global)
        {
            yield return global + new Vector3(1, 0, 0);
            yield return global - new Vector3(1, 0, 0);
            yield return global + new Vector3(0, 1, 0);
            yield return global - new Vector3(0, 1, 0);
            yield return global + new Vector3(0, 0, 1);
            yield return global - new Vector3(0, 0, 1);
        }
        static public IEnumerable<Vector2> GetNeighbors(this Vector2 coords)
        {
            yield return coords + new Vector2(1, 0);
            yield return coords - new Vector2(1, 0);
            yield return coords + new Vector2(0, 1);
            yield return coords - new Vector2(0, 1);
        }
        
        static public IEnumerable<Vector2> GetNeighbors8(this Vector2 coords)
        {
            yield return coords + new Vector2(-1, -1);
            yield return coords + new Vector2(-1, 0);
            yield return coords + new Vector2(-1, 1);
            yield return coords + new Vector2(0, -1);
            yield return coords + new Vector2(0, 1);
            yield return coords + new Vector2(1, -1);
            yield return coords + new Vector2(1, 0);
            yield return coords + new Vector2(1, 1);
        }
    
        static public IEnumerable<IntVec3> GetNeighborsDiag(this IntVec3 global)
        {
            yield return global + new IntVec3(1, 0, 0);
            yield return global + new IntVec3(-1, 0, 0);
            yield return global + new IntVec3(0, 1, 0);
            yield return global + new IntVec3(0, -1, 0);
            yield return global + new IntVec3(1, 1, 0);
            yield return global + new IntVec3(-1, 1, 0);
            yield return global + new IntVec3(1, -1, 0);
            yield return global + new IntVec3(-1, -1, 0);

            yield return global + new IntVec3(-1, -1, -1);
            yield return global + new IntVec3(-1, 0, -1);
            yield return global + new IntVec3(-1, 1, -1);
            yield return global + new IntVec3(0, -1, -1);
            yield return global + new IntVec3(0, 0, -1);
            yield return global + new IntVec3(0, 1, -1);
            yield return global + new IntVec3(1, -1, -1);
            yield return global + new IntVec3(1, 0, -1);
            yield return global + new IntVec3(1, 1, -1);

            yield return global + new IntVec3(-1, -1, 1);
            yield return global + new IntVec3(-1, 0, 1);
            yield return global + new IntVec3(-1, 1, 1);
            yield return global + new IntVec3(0, -1, 1);
            yield return global + new IntVec3(0, 0, 1);
            yield return global + new IntVec3(0, 1, 1);
            yield return global + new IntVec3(1, -1, 1);
            yield return global + new IntVec3(1, 0, 1);
            yield return global + new IntVec3(1, 1, 1);
        }
        static public IEnumerable<Vector3> GetNeighborsDiag(this Vector3 global)
        {
            yield return global + new Vector3(1, 0, 0);
            yield return global + new Vector3(-1, 0, 0);
            yield return global + new Vector3(0, 1, 0);
            yield return global + new Vector3(0, -1, 0);
            yield return global + new Vector3(1, 1, 0);
            yield return global + new Vector3(-1, 1, 0);
            yield return global + new Vector3(1, -1, 0);
            yield return global + new Vector3(-1, -1, 0);

            yield return global + new Vector3(-1, -1, -1);
            yield return global + new Vector3(-1, 0, -1);
            yield return global + new Vector3(-1, 1, -1);
            yield return global + new Vector3(0, -1, -1);
            yield return global + new Vector3(0, 0, -1);
            yield return global + new Vector3(0, 1, -1);
            yield return global + new Vector3(1, -1, -1);
            yield return global + new Vector3(1, 0, -1);
            yield return global + new Vector3(1, 1, -1);

            yield return global + new Vector3(-1, -1, 1);
            yield return global + new Vector3(-1, 0, 1);
            yield return global + new Vector3(-1, 1, 1);
            yield return global + new Vector3(0, -1, 1);
            yield return global + new Vector3(0, 0, 1);
            yield return global + new Vector3(0, 1, 1);
            yield return global + new Vector3(1, -1, 1);
            yield return global + new Vector3(1, 0, 1);
            yield return global + new Vector3(1, 1, 1);
        }
        static public Vector3 CeilingZ(this Vector3 global)
        {
            return new Vector3(global.X, global.Y, (float)Math.Ceiling(global.Z));
        }
        static public Vector3 FloorZ(this Vector3 global)
        {
            return new Vector3(global.X, global.Y, (float)Math.Floor(global.Z));
        }
        static public Vector3 Above(this Vector3 global)
        {
            return global + Vector3.UnitZ;
        }
        static public Vector3 Below(this Vector3 global)
        {
            return global - Vector3.UnitZ;
        }
        static public Vector3 West(this Vector3 global)
        {
            return global - Vector3.UnitX;
        }
        static public Vector3 East(this Vector3 global)
        {
            return global + Vector3.UnitX;
        }
        static public Vector3 North(this Vector3 global)
        {
            return global - Vector3.UnitY;
        }
        static public Vector3 South(this Vector3 global)
        {
            return global + Vector3.UnitY;
        }
    }
}
