using Start_a_Town_;
using System.IO;

namespace Project1.Framework.Interfaces
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
