using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    public class PacketReader : IDataReader
    {
        readonly BinaryReader _reader;
        readonly MemoryStream _memory;
        public PacketReader(byte[] payload)
        {
            this._memory = new(payload);
            this._reader = new(this._memory);
        }
        public PacketReader(BinaryReader reader)
        {
            this._reader = reader;
        }
        byte IDataReader.ReadByte() => this._reader.ReadByte();
        public int ReadInt32() => this._reader.ReadInt32();
        public bool ReadBoolean() => this._reader.ReadBoolean();
        public double ReadDouble() => this._reader.ReadDouble();
        public float ReadSingle() => this._reader.ReadSingle();
        public string ReadString() => this._reader.ReadString();
        public byte[] ReadBytes(int count) => this._reader.ReadBytes(count);
        public Vector3 ReadVector3() => this._reader.ReadVector3();
        public Vector2 ReadVector2() => this._reader.ReadVector2();
        public IntVec3 ReadIntVec3() => this._reader.ReadIntVec3();
        public IntVec2 ReadIntVec2() => this._reader.ReadIntVec2();
    }
}
