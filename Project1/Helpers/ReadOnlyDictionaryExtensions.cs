using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Start_a_Town_
{
    public static class ReadOnlyDictionaryExtensions
    {
        //public static void DeserializeValues<TKey, TValue>(this ReadOnlyDictionary<TKey, TValue> dic, IDataReader r, Func<TValue, TKey> keyMatcher, Action<TValue, TValue> valueUpdater) where TValue : class, ISerializableNew<TValue>
        //{
        //    var list = r.ReadList<TValue>();
        //    foreach (var item in list)
        //    {
        //        var key = keyMatcher(item);

        //        if (!dic.TryGetValue(key, out var target))
        //            throw new Exception($"Deserialized value refers to a missing key: {key}");

        //        valueUpdater(target, item);
        //    }
        //}
        //public static void DeserializeValues<TKey, TValue>(this ReadOnlyDictionary<TKey, TValue> dic, IDataReader r, Func<TValue, TKey> keyMatcher) where TValue : class, ICopyable<TValue>, ISerializableNew<TValue> 
        //{
        //    var list = r.ReadList<TValue>();
        //    foreach (var source in list)
        //    {
        //        var key = keyMatcher(source);

        //        if (!dic.TryGetValue(key, out var target))
        //            throw new Exception($"Deserialized value refers to a missing key: {key}");

        //        target.CopyFrom(source);
        //    }
        //}
        public static void Sync<TKey, TValue>(this ReadOnlyDictionary<TKey, TValue> dic, IDataReader r) where TValue : class, ICopyable<TValue>, ISerializableNew<TValue>, IKeyable<TKey>
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
        public static void Sync<TKey, TValue>(this ReadOnlyDictionary<TKey, TValue> dic, IDataWriter w) where TValue : class, ICopyable<TValue>, ISerializableNew<TValue>, IKeyable<TKey>
        {
            var toSerialize = dic.Values.Where(v=>v.ShouldCopy()).ToList();
            w.Write(toSerialize);
        }
        public static void SerializeValues<TKey, TValue>(this ReadOnlyDictionary<TKey, TValue> dic, IDataWriter w, Func<TValue, bool> filter) where TValue : class, ICopyable<TValue>, ISerializableNew<TValue>, IKeyable<TKey>
        {
            var toSerialize = dic.Values.Where(filter).ToList();
            w.Write(toSerialize);
        }
        //public static void SerializeValues<TKey, TValue>(this ReadOnlyDictionary<TKey, TValue> dic, IDataWriter w) where TValue : class, ICopyable<TValue>, ISerializableNew<TValue>, IKeyable<TKey>
        //{
        //    var toSerialize = dic.Values;//.Where(filter).ToList();
        //    w.Write(toSerialize);
        //}
        //public static void UpdateValues<TKey, TValue>(this ReadOnlyDictionary<TKey, TValue> dic, IDataReader r) where TValue : ISerializableNew<TValue>
        //{
        //    foreach (var (key, value) in dic)
        //        value.Read(r);
        //}
    }
}
