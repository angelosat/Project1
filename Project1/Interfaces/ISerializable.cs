using System.IO;

namespace Start_a_Town_
{
    public interface IDictionarySyncable<TKey, TValue> :
        IKeyable<TKey>,
        ICopyable<TValue>,
        ISerializableNew<TValue>
        where TValue : /*class, */ICopyable<TValue>, ISerializableNew<TValue>
    { }
    public interface IKeyable<T>
    {
        T GetKey();
    }
    public interface ICopyable<T> where T : ICopyable<T>, ISerializableNew<T>
    {
        T CopyFrom(T source);
        bool ShouldCopy();
    }
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
