using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    public class ObjectSnapshot
    {
        public GameObject Object;
        public Vector3 Position, Velocity, Orientation;
        public byte[] Data;

        public ObjectSnapshot(GameObject obj)
        {
            this.Object = obj;
        }
        static public void Write(GameObject obj, BinaryWriter w)
        {
            w.Write(obj.Global);
            w.Write(obj.Velocity);
            w.Write(obj.Direction);
        }
        public ObjectSnapshot Read(BinaryReader r)
        {
            this.Position = r.ReadVector3();
            this.Velocity = r.ReadVector3();
            this.Orientation = r.ReadVector3();
            return this;
        }

        public override string ToString()
        {
            return this.Object.Name.ToString() + " Position: " + this.Position + " Velocity: " + this.Velocity;
        }
    }
}
