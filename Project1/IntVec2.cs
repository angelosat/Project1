using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public struct IntVec2 : IEquatable<IntVec2>
    {
        public int X, Y;

        public IntVec2(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public IntVec2(int xy) : this(xy, xy)
        {

        }

        public IntVec2(Vector2 vector2) : this((int)Math.Round(vector2.X), (int)Math.Round(vector2.Y))
        {

        }
        public IntVec2(float x, float y) : this((int)Math.Round(x), (int)Math.Round(y))
        {

        }

        public bool Equals(IntVec2 other)
        {
            return
                this.X == other.X &&
                this.Y == other.Y;
        }
        public override bool Equals(object obj)
        {
            return obj is IntVec2 vec && this.Equals(vec);
        }
        public override int GetHashCode()
        {
            return new Vector2(this.X, this.Y).GetHashCode();
        }
        public static IntVec2 Zero = new(0);
        public static IntVec2 One = new(1);
        public static IntVec2 UnitX = new(1, 0);
        public static IntVec2 UnitY = new(0, 1);

        public static IntVec2 operator +(IntVec2 a, IntVec2 b)
        {
            return new IntVec2(a.X + b.X, a.Y + b.Y);
        }
        public static IntVec2 operator -(IntVec2 a, IntVec2 b)
        {
            return new IntVec2(a.X - b.X, a.Y - b.Y);
        }
        public static IntVec2 operator *(IntVec2 a, int k)
        {
            return new IntVec2(a.X * k, a.Y * k);
        }
        public static IntVec2 operator *(int k, IntVec2 a)
        {
            return a * k;
        }
        public static IntVec2 operator -(IntVec2 a, int b)
        {
            return new IntVec2(a.X * b, a.Y * b);
        }
        public static bool operator ==(IntVec2 a, IntVec2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        public static bool operator !=(IntVec2 a, IntVec2 b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public static implicit operator Vector2(IntVec2 a)
        {
            return new Vector2(a.X, a.Y);
        }
        public static implicit operator IntVec2(Vector2 a)
        {
            return new IntVec2(a);
        }

        public IEnumerable<IntVec2> GetNeighbors()
        {
            yield return this + new IntVec2(1, 0);
            yield return this - new IntVec2(1, 0);
            yield return this + new IntVec2(0, 1);
            yield return this - new IntVec2(0, 1);
        }

        public override string ToString()
        {
            return $"{{X:{this.X} Y:{this.Y}}}";
        }
    }
}
