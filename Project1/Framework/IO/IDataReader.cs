using Microsoft.Xna.Framework;
using Project1.Core.Base;
using Project1.Core.Simulation;
using Project1.Framework.Math;
using System.Collections.Generic;

namespace Project1.Framework.IO
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
        internal Def ReadDef();
        internal List<IntVec3> ReadListIntVec3();
        internal int[] ReadIntArray();
        internal Vector3? ReadVector3Nullable();
        internal string[] ReadStringArray();
        internal List<int> ReadListInt32();
        internal string ReadASCII();
        internal Color ReadColor();
        internal List<Vector3> ReadListVector3();
        internal TargetArgs ReadTarget(MapBase map);// => TargetArgs.Read(map, this);
    }
}
