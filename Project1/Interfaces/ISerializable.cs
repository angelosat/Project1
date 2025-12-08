using System.IO;

namespace Start_a_Town_
{
    public interface ISerializableNew
    {
        ISerializableNew Read(IDataReader r);
        void Write(IDataWriter w);
        static abstract ISerializableNew Create(IDataReader r);
    }
    public interface ISerializable
    {
        void Write(IDataWriter w);
        ISerializable Read(IDataReader r);
    }
}
