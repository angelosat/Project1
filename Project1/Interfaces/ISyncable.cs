using System.IO;

namespace Start_a_Town_
{
    public interface ISyncable
    {
        ISyncable Sync(IDataWriter w);
        ISyncable Sync(IDataReader r);
    }
}
