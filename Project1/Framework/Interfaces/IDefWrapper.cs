using Project1.Core.Base;

namespace Project1.Core.Interfaces
{
    //public interface IDefWrapper
    //{
    //    Def Def { get; }
    //}

    public interface IDefWrapper<T> where T : Def//, ISerializableNew<T>
    {
        T Def { get; }
    }
}
