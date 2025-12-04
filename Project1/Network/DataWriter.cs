using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_.Net
{
    public class DataWriter : IDataWriter
    {
        readonly BinaryWriter _writer;
        private DataWriter(MemoryStream mem)
        {
            this._writer = new(mem);
        }
        public IDataWriter Write(bool v) { this._writer.Write(v); return this; }
        public IDataWriter Write(byte[] v) { this._writer.Write(v); return this; }
        public IDataWriter Write(byte v) { this._writer.Write(v); return this; }
        public IDataWriter Write(int v) { this._writer.Write(v); return this; }
        public IDataWriter Write(float v) { this._writer.Write(v); return this; }
        public IDataWriter Write(double v) { this._writer.Write(v); return this; }
        public IDataWriter Write(long v) { this._writer.Write(v); return this; }
        public IDataWriter Write(string v) { this._writer.Write(v); return this; }
        public IDataWriter Write(IntVec3 v) { this._writer.Write(v); return this; }
        public IDataWriter Write(IntVec2 v) { this._writer.Write(v); return this; }
        public IDataWriter Write(Vector3 v) { this._writer.Write(v); return this; }
        public IDataWriter Write(Vector2 v) { this._writer.Write(v); return this; }
        public IDataWriter Write(List<int> v) { this._writer.Write(v); return this; }
        public IDataWriter WriteASCII(string v) { this._writer.WriteASCII(v); return this; }

        public IDataWriter Write(params object[] v) { this._writer.Write(v); return this; }
        internal void End() { }
    }
}
