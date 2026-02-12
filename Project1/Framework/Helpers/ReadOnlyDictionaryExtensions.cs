using System;
using System.Collections.ObjectModel;
using System.Linq;
using Project1.Framework.Serialization;

namespace Project1.Framework.Helpers
{
    public static class ReadOnlyDictionaryExtensions
    {
        public static void Sync<TKey, TValue>(this ReadOnlyDictionary<TKey, TValue> dic, IDataReader r) where TValue : ICopyable<TValue>, ISerializableNew<TValue>, IKeyable<TKey>
        {
            var list = r.ReadList<TValue>();
            foreach (var source in list)
            {
                var key = source.GetKey();

                if (!dic.TryGetValue(key, out var target))
                    throw new Exception($"Deserialized value refers to a missing key: {key}");

                target.CopyFrom(source);
            }
        }
        public static void Sync<TKey, TValue>(this ReadOnlyDictionary<TKey, TValue> dic, IDataWriter w) where TValue : ICopyable<TValue>, ISerializableNew<TValue>, IKeyable<TKey>
        {
            var toSerialize = dic.Values.Where(v=>v.ShouldCopy()).ToList();
            w.Write(toSerialize);
        }
        public static void SerializeValues<TKey, TValue>(this ReadOnlyDictionary<TKey, TValue> dic, IDataWriter w, Func<TValue, bool> filter) where TValue : class, ICopyable<TValue>, ISerializableNew<TValue>, IKeyable<TKey>
        {
            var toSerialize = dic.Values.Where(filter).ToList();
            w.Write(toSerialize);
        }
    }
}
