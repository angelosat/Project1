using System.IO;
using Microsoft.Xna.Framework;

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
        public IDataWriter Write(params object[] v) { this._writer.Write(v); return this; }
        internal static PacketBuilder Create(BinaryWriter w, int pType)
        {
            return new(w, pType);
        }
        internal void End() { }
    }

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
        public IDataWriter Write(params object[] v) { this._writer.Write(v); return this; }
        internal void End() { }
    }
}
