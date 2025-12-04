using Microsoft.Xna.Framework;

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

    }
}
