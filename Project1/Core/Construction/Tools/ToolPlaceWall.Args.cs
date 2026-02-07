using System.IO;
using Microsoft.Xna.Framework;
using Project1.Core.Helpers;

namespace Project1.Core.Modules.Construction
{
    public partial class ToolPlaceWall
    {
        public class Args
        {
            public Vector3 Begin, End;
            public bool ModifierKey;
            public Args(Vector3 begin, Vector3 end, bool modkey)
            {
                this.Begin = begin;
                this.End = end;
                this.ModifierKey = modkey;
            }
            public void Write(BinaryWriter w)
            {
                w.Write(this.Begin);
                w.Write(this.End);
                w.Write(this.ModifierKey);
            }
            public Args(BinaryReader r)
            {
                this.Begin = r.ReadVector3();
                this.End = r.ReadVector3();
                this.ModifierKey = r.ReadBoolean();
            }
        }
      



       
    }
}
