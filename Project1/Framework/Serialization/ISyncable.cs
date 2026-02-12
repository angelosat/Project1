namespace Project1.Framework.Serialization
{
    public interface ISyncable
    {
        ISyncable Sync(IDataWriter w);
        ISyncable Sync(IDataReader r);
    }

    public interface ICopyable
    {
        void CopyFrom(ICopyable source);
    }
}
