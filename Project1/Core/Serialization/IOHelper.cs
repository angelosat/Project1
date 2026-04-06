using System.Collections.Generic;
using System.IO;
using System.Linq;
using Project1.Framework.Serialization;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using System;

namespace Project1.Core.Serialization
{
    static class IOHelper
    {

        public static ICollection<Def> Read(this ICollection<Def> list, BinaryReader r)
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(Def.GetDef(r));
            return list;
        }
        public static T ReadDef<T>(this BinaryReader r) where T : Def
        {
            return Def.Get<T>(r.ReadString());
        }
        public static ICollection<T> ReadDefs<T>(this ICollection<T> list, IDataReader r) where T : Def
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(Def.Get<T>(r));
            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="net">pass the net peer to resolve targets initially</param>
        /// <returns></returns>
        [Obsolete]
        public static List<TargetArgs> ReadListTargets(this IDataReader r, NetEndpoint net = null)
        {
            var count = r.ReadInt32();
            var list = new List<TargetArgs>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(TargetArgs.Read(net, r));
            }
            return list;
        }
        public static List<TargetArgs> ReadListTargets(this IDataReader r, MapBase map)
        {
            var count = r.ReadInt32();
            var list = new List<TargetArgs>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(TargetArgs.Read(map, r));
            }
            return list;
        }
        public static T ReadTargets<T>(this T collection, MapBase map, IDataReader r)
           where T : ICollection<TargetArgs>, new()
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                collection.Add(TargetArgs.Read(map, r));
            return collection;
        }
        public static Dictionary<T, U> Sync<T, U>(this Dictionary<T, U> dic, IDataWriter w) where U : ISerializable where T : Def
        {
            foreach (var vk in dic)
            {
                vk.Key.Write(w);
                vk.Value.Write(w);
            }
            return dic;
        }
        public static Dictionary<T, U> Sync<T, U>(this Dictionary<T, U> dic, IDataReader r) where U : ISerializable where T : Def
        {
            for (int i = 0; i < dic.Count; i++)
            {
                var def = Def.GetDef(r.ReadString()) as T;
                dic[def].Read(r);
            }
            return dic;
        }
        public static Dictionary<T, U> SyncNew<T, U>(this Dictionary<T, U> dic, IDataWriter w) where U : ISerializableNew<U> where T : Def
        {
            foreach (var vk in dic)
            {
                vk.Key.Write(w);
                vk.Value.Write(w);
            }
            return dic;
        }
        public static Dictionary<T, U> SyncNew<T, U>(this Dictionary<T, U> dic, IDataReader r) where U : ISerializableNew<U> where T : Def
        {
            for (int i = 0; i < dic.Count; i++)
            {
                var def = Def.GetDef(r.ReadString()) as T;
                dic[def].Read(r);
            }
            return dic;
        }
        public static void Write(this BinaryWriter w, List<TargetArgs> list)
        {
            var count = list.Count;
            w.Write(count);
            foreach (var i in list)
                i.Write(w);
        }
        public static void Write(this BinaryWriter w, IEnumerable<TargetArgs> list)
        {
            w.Write(list.ToList());
        }

        public static void Write(this BinaryWriter w, Def def)
        {
            w.Write(def.Name);
        }
        public static void Write(this BinaryWriter w, TargetArgs target)
        {
            target.Write(w);
        }
        public static void Write(this BinaryWriter writer, PacketType packetType)
        {
            writer.Write((int)packetType);
        }
        public static void WriteDefs<T>(this ICollection<T> list, IDataWriter w) where T : Def
        {
            var count = list.Count;
            w.Write(count);
            foreach (var i in list)
                i.Write(w);
        }
        public static void Write(this ICollection<Def> list, IDataWriter w)
        {
            var count = list.Count;
            w.Write(count);
            foreach (var i in list)
                i.Write(w);
        }
        public static void Write(this BinaryWriter w, ICollection<TargetArgs> list)
        {
            var count = list.Count;
            w.Write(count);
            foreach (var i in list)
                w.Write(i);
        }
    }
}
