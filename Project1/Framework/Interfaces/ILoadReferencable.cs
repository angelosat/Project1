namespace Project1.Framework.Interfaces
{
    public interface ILoadReferencable<T> : ISaveable where T : new()
    {
        string GetUniqueLoadID();
    }
    public interface ILoadReferencable : ISaveable
    {
        string GetUniqueLoadID();
    }
}
