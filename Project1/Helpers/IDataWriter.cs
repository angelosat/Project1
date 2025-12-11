using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms.VisualStyles;

namespace Start_a_Town_
{
    public interface IDataWriter
    {
        BinaryWriter ww { get; }
        internal IDataWriter Write(byte v);
        internal IDataWriter Write(string v);
        internal IDataWriter Write(bool v);
        internal IDataWriter Write(int v);
        internal IDataWriter Write(float v);
        internal IDataWriter Write(ulong v) { this.ww.Write(v); return this; }
        internal IDataWriter Write(double v);
        internal IDataWriter Write(byte[] v);
        internal IDataWriter Write(long v);
        internal IDataWriter Write(Vector3 v);
        internal IDataWriter Write(Vector2 v);
        internal IDataWriter Write(IntVec3 v);
        internal IDataWriter Write(IntVec3? v) { this.ww.Write(v); return this; }

        internal IDataWriter Write(IntVec2 v);
        internal IDataWriter Write(List<int> v);
        internal IDataWriter Write(ICollection<int> v) { this.ww.Write(v); return this; }
        public IDataWriter Write(int[] v) { this.ww.Write(v); return this; }
        internal IDataWriter Write(Def def) { this.ww.Write(def); return this; }
        internal IDataWriter Write(ICollection<IntVec3> list);
        internal IDataWriter Write(ICollection<TargetArgs> list);
        internal IDataWriter Write<T>(ICollection<T> list) where T : ISerializableNew<T>;
        internal IDataWriter Write(string[] strings) { this.ww.Write(strings); return this; }
        internal IDataWriter Write(Color color) { this.ww.Write(color); return this; }

        internal IDataWriter WriteASCII(string v);
        internal IDataWriter Write(IntVec3[] v);

    }
}
