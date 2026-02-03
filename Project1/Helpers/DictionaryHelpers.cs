using Project1.Framework.Base;
using System;
using System.Collections.Generic;
using System.IO;

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
        public static void ReadDefWrappers<TKey, TValue>(this IDataReader r, Dictionary<TKey, TValue> dic) where TValue : IDefWrapper<TKey>, ISerializableNew<TValue> where TKey : Def
        {
            dic.Clear();
            var values = new List<TValue>();
            r.ReadNewInto(values);
            foreach (var val in values)
                dic.Add(val.Def, val);
        }
        public static Dictionary<TKey, TValue> ReadDefWrappers<TKey, TValue>(this IDataReader r) where TValue : IDefWrapper<TKey>, ISerializableNew<TValue> where TKey : Def
        {
            Dictionary<TKey, TValue> dic = [];
            var values = new List<TValue>();
            r.ReadNewInto(values);
            foreach (var val in values)
                dic.Add(val.Def, val);
            return dic;
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
            var values = tag.LoadArrayNewNew<TValue>();
            foreach(var value in values)
                dic.Add(keySelector(value), value);
        }

        public static void SaveValues<TKey, TValue>(this SaveTag tag, Dictionary<TKey, TValue> dic, string name) where TValue : ISaveableNewNew<TValue>
        {
            //tag.Add(dic.Values.Save(name));
            tag.Save(name, dic.Values);
        }
        public static void LoadValuesWithInferredKeys<TKey, TValue>(this SaveTag tag, Dictionary<TKey, TValue> dic, Func<TValue, TKey> keySelector) where TValue : ISaveableNewNew<TValue>, new()
        {
            dic.Clear();
            var values = tag.LoadArrayNewNew<TValue>();
            foreach (var n in values)
                dic.Add(keySelector(n), n);
            //var taglist = tag[name].Value as List<SaveTag>;
            //foreach (var t in taglist)
            //{
            //    var n = (TValue)TValue.Create(t);
            //    dic.Add(keySelector(n), n);
            //}
        }
        public static void SaveDefWrappers<TKey, TValue>(this SaveTag tag, string name, Dictionary<TKey, TValue> dic) where TValue : ISaveableNewNew<TValue>, IDefWrapper<TKey>, new() where TKey : Def
        {
            tag.Save(name, dic.Values);
        }
        public static void LoadDefWrappers<TKey, TValue>(this SaveTag tag, Dictionary<TKey, TValue> dic) where TValue : ISaveableNewNew<TValue>, IDefWrapper<TKey>, new() where TKey : Def
        {
            dic.Clear();
            var values = tag.LoadArrayNewNew<TValue>();
            foreach (var n in values)
                if(!dic.TryAdd(n.Def, n))
                    throw new InvalidDataException($"Duplicate def '{n.Def}' while loading {typeof(TValue).Name}");
        }
        public static void LoadDefWrappersCopyFrom<TKey, TValue>(this SaveTag tag, Dictionary<TKey, TValue> dic) where TValue : ICopyable, ISaveableNewNew<TValue>, IDefWrapper<TKey>, new() where TKey : Def
        {
            dic.Clear();
            var values = tag.LoadArrayNewNew<TValue>();
            foreach (var n in values)
                    if (dic.TryGetValue(n.Def, out var nvalue)) nvalue.CopyFrom(n);
                else
                    throw new InvalidDataException($"Missing def '{n.Def}' while loading {typeof(TValue).Name}");
        }
        public static void LoadDefWrappers<TKey, TValue>(this SaveTag tag, string name, Dictionary<TKey, TValue> dic) where TValue : ISaveableNewNew<TValue>, IDefWrapper<TKey>, new() where TKey : Def
        {
            tag[name].LoadDefWrappers(dic);
            //dic.Clear();
            //var values = tag[name].LoadListNewNew<TValue>();
            //foreach (var n in values)
            //    if (!dic.TryAdd(n.Def, n))
            //        throw new InvalidDataException($"Duplicate def '{n.Def}' while loading {typeof(TValue).Name}");
        }
        //public static Dictionary<TKey, TValue> LoadDefWrappers<TKey, TValue>(this SaveTag tag, string name) where TValue : ISaveableNewNew<TValue>, IDefWrapper<TKey>, new() where TKey : Def
        //{
        //    return tag[name].LoadDefWrappers<TKey, TValue>();
        //    //Dictionary<TKey, TValue> dic = [];
        //    //var values = tag[name].LoadListNewNew<TValue>();
        //    //foreach (var n in values)
        //    //    if (!dic.TryAdd(n.Def, n))
        //    //        throw new InvalidDataException($"Duplicate def '{n.Def}' while loading {typeof(TValue).Name}");
        //    //return dic;
        //}
        //public static Dictionary<TKey, TValue> LoadDefWrappers<TKey, TValue>(this SaveTag tag) where TValue : ISaveableNewNew<TValue>, IDefWrapper<TKey>, new() where TKey : Def
        //{
        //    Dictionary<TKey, TValue> dic = [];
        //    //var values = tag.LoadListNewNew<TValue>();
        //    //foreach (var n in values)
        //    //    if (!dic.TryAdd(n.Def, n))
        //    //        throw new InvalidDataException($"Duplicate def '{n.Def}' while loading {typeof(TValue).Name}");
        //    tag.LoadDefWrappers(dic);
        //    return dic;
        //}
    }
}