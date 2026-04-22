using Project1.Core.Entities;
using Project1.Core.Simulation;
using Project1.Core.Systems.Crafting;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Project1.Core.Helpers;

internal static class IOHelpers
{
    public static EntityRefId ReadEntityRefId(this IDataReader r) => new(r.ReadInt32());
    public static MapId ReadMapId(this IDataReader r) => new(r.ReadInt32());
    public static T ReadId<T>(this IDataReader r) where T : IStructIdInt<T> => T.Create(r.ReadInt32());
    public static List<T> ReadListId<T>(this IDataReader r) where T : IStructIdInt<T>
    {
        var length = r.ReadInt32();
        var list = new List<T>(length);
        for (int i = 0; i < length; i++)
            list.Add(T.Create(r.ReadInt32()));
        return list;
    }
    public static List<EntityRefId> ReadListEntityRefId(this IDataReader r)
    {
        var count = r.ReadInt32();
        var list = new List<EntityRefId>(count);
        for (int i = 0; i < count; i++)
            list.Add(r.ReadEntityRefId());
        return list;
    }
    public static T ReadDef<T>(this IDataReader r) where T : Def => Def.Get<T>(r.ReadString());
    public static Def ReadDef(this IDataReader r) => Def.GetDef(r.ReadString());
    public static T TryReadDef<T>(this IDataReader r) where T : Def => r.ReadString() is string defName && !defName.IsNullEmptyOrWhiteSpace() ? Def.Get<T>(defName) : null!;
    public static InteractionTarget ReadTarget(this IDataReader r, WorldBase world) => InteractionTarget.Read(world, r);
    public static IDataWriter Write(this IDataWriter w, ICollection<EntityRefId> v)
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
    public static IDataWriter Write(this IDataWriter w, Def def) 
    {
        w.Write(def?.Name ?? string.Empty); 
        return w; 
    }
    public static IDataWriter Write(this IDataWriter w, ICollection<Def> defs)
    {
        w.Write(defs.Count);
        foreach (var def in defs)
            w.Write(def);
        return w;
    }
    public static List<Def> ReadListDef(this IDataReader r)
    {
        List<Def> list = [];
        var count = r.ReadInt32();
        for (int i = 0; i < count; i++)
            list.Add(r.ReadDef());
        return list;
    }
    public static List<T> ReadListDef<T>(this IDataReader r) where T : Def
    {
        List<T> list = [];
        var count = r.ReadInt32();
        for (int i = 0; i < count; i++)
            list.Add(r.ReadDef<T>());
        return list;
    }
    public static IDataWriter Write(this IDataWriter w, ICollection<InteractionTarget> list)
    {
        w.Write(list.Count);
        foreach (var i in list)
            i.Write(w);
        return w;
    }
    public static IDataWriter Write(this IDataWriter w, EntityComp comp) { comp.Write(w); return w; }
    public static IDataWriter Write(this IDataWriter w, InteractionTarget target) { target.Write(w); return w; }
    public static void SaveDef<T>(this SaveTag tag, string name, T def) where T : Def
    {
        tag.Add(def.Save(name));
    }
    public static void Save<T>(this SaveTag tag, string name, T def) where T : Def
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
    public static Dictionary<TKey, TValue> LoadDefWrappers<TKey, TValue>(this SaveTag tag, string name) where TValue : ISaveableNewNew<TValue>, IDefWrapper<TKey>, new() where TKey : Def
    {
        var dic = new Dictionary<TKey, TValue>();
        tag[name].LoadDefWrappers(dic);
        return dic;
    }

    extension(SaveTag tag)
    {
        public EntityRefId LoadEntityRefId(string name) => (EntityRefId)(int)tag[name].Value;
        public List<EntityRefId> LoadListEntityRefId(string name) => [.. tag.LoadListInt(name).Select(i => (EntityRefId)i)];
        public void Save(string name, ICollection<EntityRefId> list)
        {
            var asints = list.Select(i => (int)i).ToList();
            tag.Save(name, asints);
        }
    }
}
