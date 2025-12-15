using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public static class DictionaryHelpers
    {
        public static void ReadValuesWithInferredKeys<TKey, TValue>(this IDataReader r, Dictionary<TKey, TValue> dic, Func<TValue, TKey> keySelector) where TValue : ISerializableNew<TValue>
        {
            dic.Clear();
            var values = new List<TValue>();
            r.ReadNewInto(values);
            foreach (var val in values)
                dic.Add(keySelector(val), val);
        }
        public static void WriteValues<TKey, TValue>(this IDataWriter w, Dictionary<TKey, TValue> dic) where TValue : ISerializableNew<TValue>
        {
            w.Write(dic.Values);
        }

        public static SaveTag SaveValues<TKey, TValue>(this Dictionary<TKey, TValue> dec, string name = "") where TValue : ISaveableNewNew<TValue>, new()
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Compound);
            foreach (var item in dec.Values)
                list.Add(item.Save());
            return list;
        }
        public static void LoadValuesWithInferredKeys<TKey, TValue>(this Dictionary<TKey, TValue> dic, SaveTag tag, Func<TValue, TKey> keySelector) where TValue : ISaveableNewNew<TValue>, new()
        {
            dic.Clear();
            //var values = new List<TValue>();
            var values = tag.LoadListNewNew<TValue>();
            foreach(var value in values)
            {
                dic.Add(keySelector(value), value);
            }
        }
    }
}