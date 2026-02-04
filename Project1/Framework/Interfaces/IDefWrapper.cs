using Project1.Framework.Base;

namespace Project1.Framework.Interfaces
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
