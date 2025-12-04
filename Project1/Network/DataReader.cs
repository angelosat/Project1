using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;

#nullable enable

namespace Start_a_Town_.Net
{
    public class DataReader : IDataReader, IDisposable
    {
        readonly BinaryReader _reader;
        readonly MemoryStream? _memory;
        bool _disposed;
        public DataReader(byte[] payload)
        {
            this._memory = new(payload);
            this._reader = new(this._memory);
        }
        public DataReader(Stream payload)
        {
            this._reader = new(payload);
            this._memory = null;
        }
        public DataReader(BinaryReader reader)
        {
            this._reader = reader;
        }
        public void Dispose()
        {
            if (_disposed)
                return;
            this._reader.Dispose();
        }
        public long Length => this._reader.BaseStream.Length;
        public long Position => this._reader.BaseStream.Position;
        public byte ReadByte() => this._reader.ReadByte();
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
        public ulong ReadUInt64() => this._reader.ReadUInt64();
        public long ReadInt64() => this._reader.ReadInt64();
        public T ReadDef<T>() where T : Def => Def.GetDef<T>(this._reader.ReadString());
        public List<IntVec3> ReadListIntVec3() => this._reader.ReadListIntVec3();
        public int[] ReadIntArray() => this._reader.ReadIntArray();
        public Vector3? ReadVector3Nullable() => this._reader.ReadVector3Nullable();
        public string[] ReadStringArray() => this._reader.ReadStringArray();
        public List<int> ReadListInt() => this._reader.ReadListInt();
        public string ReadASCII() => this._reader.ReadASCII();
        public Color ReadColor() => this._reader.ReadColor();
        public List<Vector3> ReadListVector3() => this._reader.ReadListVector3();

    }
}
