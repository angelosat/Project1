using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_
{
    public interface IDataReader
    {
        internal byte ReadByte();
        internal string ReadString();
        internal bool ReadBoolean();
        internal int ReadInt32();
        internal float ReadSingle();
        internal byte[] ReadBytes(int count);
        internal double ReadDouble();
        internal Vector3 ReadVector3();
        internal Vector2 ReadVector2();
        internal IntVec3 ReadIntVec3();
        internal IntVec2 ReadIntVec2();
        internal ulong ReadUInt64();
        internal long ReadInt64();
        internal T ReadDef<T>() where T : Def;
        internal List<IntVec3> ReadListIntVec3();
        internal int[] ReadIntArray();
        internal Vector3? ReadVector3Nullable();
        internal string[] ReadStringArray();
        internal List<int> ReadListInt();
        internal string ReadASCII();
        internal Color ReadColor();
        internal List<Vector3> ReadListVector3();

    }
}
