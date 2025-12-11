using System.IO;

namespace Start_a_Town_
{
    public interface ISerializableNew<T> where T: ISerializableNew<T>
    {
        T Read(IDataReader r);
        void Write(IDataWriter w);
        static abstract T Create(IDataReader r);
    }
    public interface ISerializable
    {
        void Write(IDataWriter w);
        ISerializable Read(IDataReader r);
    }
}
