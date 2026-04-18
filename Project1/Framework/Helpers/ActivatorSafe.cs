using System;

namespace Project1.Framework.Helpers;

public static class ActivatorSafe<T> where T : class
{
    //static public T CreateInstance(Type type, params object[] args) => Activator.CreateInstance(type, args) as T ?? throw new NullReferenceException($"{type} not a valid {nameof(T)}");
    static public T CreateInstance(Type type, params object[] args) => Activator.CreateInstance(type, nonPublic: true) as T ?? throw new NullReferenceException($"{type} not a valid {nameof(T)}");
}
