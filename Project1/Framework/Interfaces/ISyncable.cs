using Project1.Framework.IO;
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
