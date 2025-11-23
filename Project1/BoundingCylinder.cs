using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    struct BoundingCylinder
    {
        public Vector3 Position;
        public float Radius;
        public float Height;

        public BoundingCylinder(Vector3 global, float radius, float height)
        {
            this.Position = global;
            this.Radius = radius;
            this.Height = height;
        }

        public bool Intersects(Ray ray)
        {
            var checkbox = new BoundingBox(this.Position + new Vector3(-Radius, -Radius, 0), this.Position + new Vector3(Radius, Radius, this.Height));
            var intersection = ray.Intersects(checkbox);
            if (!intersection.HasValue)
                return false;

            Vector3 distance = new Vector3(this.Position.XY() - ray.Position.XY(), 0); 
            float scalar = Vector3.Dot(distance, ray.Direction);
            Vector3 ab = ray.Direction * Math.Abs(scalar);
            Vector3 rejection = distance - ab;

            float rejectionLength = rejection.Length();
            return rejectionLength <= this.Radius;
        }
        public bool Contains(Vector3 vec)
        {
            var z = vec.Z;
            if (z < this.Position.Z)
                return false;
            if (z > this.Position.Z + Height)
                return false;
            if (Vector2.Distance(vec.XY(), this.Position.XY()) > this.Radius)
                return false;
            return true;
        }
        public bool Intersects(BoundingCylinder c2)
        {
            float
                a0 = this.Position.Z,
                a1 = this.Position.Z + this.Height,
                b0 = c2.Position.Z,
                b1 = c2.Position.Z + c2.Height;
            if (b0 < a0 && b1 < a0 || b0 > a1 && b1 > a1)
                return false;
            var d = Vector2.Distance(this.Position.XY(), c2.Position.XY());
            return d <= this.Radius + c2.Radius;
        }
        public static BoundingCylinder Create(GameObject obj)
        {
            // TODO: half the radius maybe
            return new BoundingCylinder(obj.Global, 0.5f, obj.Physics.Height);
        }
    }
}
