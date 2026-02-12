using System.Collections.Generic;
using System.IO;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Core.Entities;
using Project1.Core.Helpers.Structs;
using Project1.Core.Simulation;

namespace Project1.Core.Helpers
{
    internal static class IOHelpers
    {
        public static int ReadEntityRefId(this IDataReader r) => new EntityRefId(r.ReadInt32());
        public static T ReadDef<T>(this IDataReader r) where T : Def => Def.GetDef<T>(r.ReadString());
        public static Def ReadDef(this IDataReader r) => Def.GetDef(r.ReadString());
        public static T TryReadDef<T>(this IDataReader r) where T : Def => r.ReadString() is string defName && !defName.IsNullEmptyOrWhiteSpace() ? Def.GetDef<T>(defName) : null!;
        public static TargetArgs ReadTarget(this IDataReader r, MapBase map) => TargetArgs.Read(map, r);

        public static IDataWriter Write(this IDataWriter w, List<EntityRefId> v)
        {
            w.Write(v.Count);
            foreach (var i in v)
            {
                if (i == 0)
                    throw new System.Exception();
                w.Write(i);
            }
            return w;
        }
        public static IDataWriter Write(this IDataWriter w, Def def) { w.Write(def.Name); return w; }
        public static IDataWriter Write(this IDataWriter w, ICollection<TargetArgs> list)
        {
            w.Write(list.Count);
            foreach (var i in list)
                i.Write(w);
            return w;
        }
        public static IDataWriter Write(this IDataWriter w, EntityComp comp) { comp.Write(w); return w; }
        public static IDataWriter Write(this IDataWriter w, TargetArgs target) { target.Write(w); return w; }

        public static void SaveDef<T>(this SaveTag tag, string name, T def) where T : Def
        {
            tag.Add(def.Save(name));
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
        public static void SaveDefWrappers<TKey, TValue>(this SaveTag tag, string name, Dictionary<TKey, TValue> dic) where TValue : ISaveableNewNew<TValue>, IDefWrapper<TKey>, new() where TKey : Def
        {
            tag.Save(name, dic.Values);
        }
        public static void LoadDefWrappers<TKey, TValue>(this SaveTag tag, Dictionary<TKey, TValue> dic) where TValue : ISaveableNewNew<TValue>, IDefWrapper<TKey>, new() where TKey : Def
        {
            dic.Clear();
            var values = tag.LoadArrayNewNew<TValue>();
            foreach (var n in values)
                if (!dic.TryAdd(n.Def, n))
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
        }
    }
}
