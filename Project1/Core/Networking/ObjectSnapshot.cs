using System.IO;
using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Framework.Serialization;

namespace Project1.Core.Networking
{
    public class ObjectSnapshot(int refID)
    {
        public int RefID = refID;
        public Vector3 Position, Velocity, Orientation;

        static public void Write(GameObject obj, IDataWriter w)
        {
            w.Write(obj.Global);
            w.Write(obj.Velocity);
            w.Write(obj.Direction);
        }
        //public ObjectSnapshot Read(BinaryReader r)
        public ObjectSnapshot Read(IDataReader r)
        {
            this.Position = r.ReadVector3();
            this.Velocity = r.ReadVector3();
            this.Orientation = r.ReadVector3();
            return this;
        }
      
        public override string ToString()
        {
            return $"RefID: {this.RefID} Position: {this.Position} Velocity: {this.Velocity} Orientation: {this.Orientation}";
        }
    }
}
