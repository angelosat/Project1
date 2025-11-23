using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Start_a_Town_
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EnsureStaticCtorCall : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ImportConfig : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class PlayerSettings : Attribute
    {
    }
    public static class EnsureInitHelper
    { 
        static Type[] CachedTypes;
        public static void Init()
        {
            CachedTypes ??= Assembly.GetExecutingAssembly().GetTypes();
            foreach (var tt in CachedTypes.Where(t => t.GetCustomAttribute<EnsureStaticCtorCall>() is not null))
                RuntimeHelpers.RunClassConstructor(tt.TypeHandle);
        }
        public static void ImportConfig()
        {
            CachedTypes ??= Assembly.GetExecutingAssembly().GetTypes();
            foreach (var tt in CachedTypes.Where(t => t.GetCustomAttribute<ImportConfig>() is not null))
                tt.GetMethod("Import", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        }
    }
}
