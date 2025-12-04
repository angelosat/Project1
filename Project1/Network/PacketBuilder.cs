using System.IO;

namespace Start_a_Town_.Net
{
    public class PacketBuilder
    {
        readonly BinaryWriter _writer;
        private PacketBuilder(BinaryWriter writer, int pType)
        {
            this._writer = writer;
            writer.Write(pType);
        }
        internal void Write(bool v) => this._writer.Write(v);
        internal void Write(byte v) => this._writer.Write(v);
        internal void Write(int v) => this._writer.Write(v);
        internal void Write(float v) => this._writer.Write(v);
        internal void Write(string v) => this._writer.Write(v);
        internal void Write(IntVec3 v) => this._writer.Write(v);
        internal void Write(params object[] v) => this._writer.Write(v);
        internal static PacketBuilder Create(BinaryWriter w, int pType)
        {
            return new(w, pType);
        }
        internal void End() { }
    }
}
