namespace Project1.Core
{
    public interface IDefWrapper<T> where T : Def
    {
        T Def { get; }
    }
}