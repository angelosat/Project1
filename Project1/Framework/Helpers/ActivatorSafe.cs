using System;
namespace Project1.Core.Helpers
{
    public static class ActivatorSafe<T> where T : class
    {
        static public T CreateInstance(Type type) => Activator.CreateInstance(type) as T ?? throw new NullReferenceException($"{type} not a valid {nameof(T)}");
    }
}
