using Project1.Core;

namespace Project1.Framework.Base
{
    public interface IDefWrapper<T> where T : Def
    {
        T Def { get; }
    }
}