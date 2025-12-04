using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_.Net
{
    public class PacketBuilder : IDataWriter
    {
        readonly BinaryWriter _writer;
        private PacketBuilder(BinaryWriter writer, int pType)
        {
            this._writer = writer;
            writer.Write(pType);
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
        public IDataWriter Write(IntVec3[] v) { this._writer.Write(v); return this; }

        public IDataWriter WriteASCII(string v) { this._writer.WriteASCII(v); return this; }

        public IDataWriter Write(params object[] v) { this._writer.Write(v); return this; }
        internal static PacketBuilder Create(BinaryWriter w, int pType)
        {
            return new(w, pType);
        }
        internal void End() { }
    }
}
