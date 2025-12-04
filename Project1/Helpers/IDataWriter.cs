using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public interface IDataWriter
    {
        internal IDataWriter Write(byte v);
        internal IDataWriter Write(string v);
        internal IDataWriter Write(bool v);
        internal IDataWriter Write(int v);
        internal IDataWriter Write(float v);
        internal IDataWriter Write(double v);
        internal IDataWriter Write(byte[] v);
        internal IDataWriter Write(long v);
        internal IDataWriter Write(Vector3 v);
        internal IDataWriter Write(Vector2 v);
        internal IDataWriter Write(IntVec3 v);
        internal IDataWriter Write(IntVec2 v);
        internal IDataWriter Write(List<int> v);
        internal IDataWriter Write(params object[] v);

    }
}
