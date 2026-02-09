using Project1.Framework.Serialization;
using System.IO;

namespace Project1.Core.Interfaces
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
