using System.IO;

namespace Start_a_Town_
{
    public interface ISerializable
    {
        void Write(BinaryWriter w);
        ISerializable Read(IDataReader r);
    }
}
