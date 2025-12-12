using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Start_a_Town_
{
    static class IOHelper
    {
        public static byte[] GetBytes(this Vector2 vector)
        {
            byte[] data =
                BitConverter.GetBytes(vector.X)
                .Concat(BitConverter.GetBytes(vector.Y))
                .ToArray();
            return data;
        }
        public static byte[] GetBytes(this Vector3 vector)
        {
            byte[] data =
                BitConverter.GetBytes(vector.X)
                .Concat(BitConverter.GetBytes(vector.Y))
                .Concat(BitConverter.GetBytes(vector.Z))
                .ToArray();
            return data;
        }
        /// <summary>
        /// T must have a constructor that accepts a BinaryReader as a single parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="r"></param>
        public static void Initialize<T>(this ICollection<T> collection, IDataReader r)
            where T : class, ISerializable
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                collection.Add(Activator.CreateInstance(typeof(T), r) as T);
        }

        /// <summary>
        /// T must have a constructor that accepts a BinaryReader as the first parameter, and args as the rest parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="r"></param>
        /// <param name="args"></param>
        public static void Initialize<T>(this ICollection<T> collection, IDataReader r, params object[] args)
            where T : class, ISerializable
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                collection.Add(Activator.CreateInstance(typeof(T), new[] { r }.Concat(args).ToArray()) as T);
        }
        public static void InitializeAbstract<T>(this ICollection<T> collection, IDataReader r, params object[] args)
            where T : class, ISerializable
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var typeName = r.ReadString();
                if (args.Length == 0)
                    collection.Add((Activator.CreateInstance(Type.GetType(typeName)) as T).Read(r) as T);
                else
                    collection.Add((Activator.CreateInstance(Type.GetType(typeName), args) as T).Read(r) as T);
            }
        }
        public static void InitializeNew<T>(this ICollection<T> collection, IDataReader r)
            where T : class, ISerializableNew<T>, new()
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                collection.Add(new T().Read(r));
        }
        public static void InitializeNew<T>(this IList<T> collection, IDataReader r)
            where T : class, ISerializableNew<T>, new()
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                collection.Add(new T().Read(r));
        }
        public static void LoadFrom<T>(this ICollection<T> collection, IDataReader r)
            where T : class, ISerializableNew<T>
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                collection.Add((T)T.Create(r));
        }
        public static Dictionary<int, int> Read(this Dictionary<int, int> dic, IDataReader r)
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                dic.Add(r.ReadInt32(), r.ReadInt32());
            }
            return dic;
        }
        public static T[] Read<T>(this T[] array, IDataReader r)
            where T : class, ISerializableNew<T>
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                array[i].Read(r);
            return array;
        }
        public static T Read<T>(this T collection, IDataReader r)
            where T : ICollection<int>
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                collection.Add(r.ReadInt32());
            return collection;
        }
        public static ICollection<IntVec3> Read(this ICollection<IntVec3> list, IDataReader r)
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(r.ReadIntVec3());
            return list;
        }

        public static ICollection<Def> Read(this ICollection<Def> list, BinaryReader r)
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(Def.GetDef(r));//.ReadString()));
            return list;
        }

        public static ICollection<U> Read<U>(this ICollection<U> collection, IDataReader r, params object[] args)
            where U : class, ISerializableNew<U>
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                collection.Add(U.Create(r));
            return collection;
        }
        public static Dictionary<T, U> Read<T, U>(this Dictionary<T, U> dic, IDataReader r, Func<U, T> keySelector, Action<U> initializer)
            where U : class, ISerializableNew<U>, new()
        {
            var list = new List<U>().Read(r);
            foreach (var i in list)
            {
                dic[keySelector(i)] = i;
                initializer.Invoke(i);
            }
            return dic;
        }
        public static Dictionary<T, U> Read<T, U>(this Dictionary<T, U> dic, IDataReader r, Func<U, T> keySelector, params object[] constructorArgs)
            where U : class, ISerializableNew<U>, new()
        {
            var list = new List<U>().Read(r, constructorArgs);
            foreach (var i in list)
            {
                dic[keySelector(i)] = i;
            }
            return dic;
        }
        public static T[] ReadArray<T>(this IDataReader r, params object[] constructorParams) where T : class, ISerializable
        {
            var count = r.ReadInt32();
            var list = new T[count];
            for (int i = 0; i < count; i++)
            {
                var instance = Activator.CreateInstance(typeof(T), constructorParams) as T;
                list[i] = instance.Read(r) as T;
            }
            return list;
        }
        public static T[] ReadArrayNew<T>(this IDataReader r, params object[] constructorParams) where T : class, ISerializableNew<T>, new()
        {
            var count = r.ReadInt32();
            var list = new T[count];
            for (int i = 0; i < count; i++)
                list[i] = new T().Read(r);
            return list;
        }
        public static string ReadASCII(this BinaryReader reader)
        {
            return Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadInt32()));
        }
        public static Dictionary<T, U> ReadByValueAbstractTypes<T, U>(this Dictionary<T, U> dic, IDataReader r, Func<U, T> keySelector, params object[] ctorArgs) where U : class, ISerializable
        {
            var items = r.ReadListAbstract<U>(ctorArgs);
            foreach (var i in items)
                dic.Add(keySelector(i), i);
            return dic;
        }
        public static ObservableDictionary<T, U> ReadByValueAbstractTypes<T, U>(this ObservableDictionary<T, U> dic, IDataReader r, Func<U, T> keySelector, params object[] ctorArgs) where U : class, ISerializable
        {
            var items = r.ReadListAbstract<U>(ctorArgs);
            foreach (var i in items)
                dic.Add(keySelector(i), i);
            return dic;
        }
        public static T ReadDef<T>(this BinaryReader r) where T : Def
        {
            return Def.GetDef<T>(r.ReadString());
        }
        public static ICollection<T> ReadDefs<T>(this ICollection<T> list, IDataReader r) where T : Def
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(Def.GetDef<T>(r));//.ReadString()));
            return list;
        }
        public static void ReadImmutable<T>(this IList<T> collection, IDataReader r)
            where T : class, ISerializable//, new()
        {
            //var count = r.ReadInt32();
            for (int i = 0; i < collection.Count; i++)
                collection[i].Read(r);
        }
        public static int[] ReadIntArray(this BinaryReader r)
        {
            var count = r.ReadInt32();
            var list = new int[count];
            for (int i = 0; i < count; i++)
            {
                list[i] = r.ReadInt32();
            }
            return list;
        }
        public static IntVec2 ReadIntVec2(this BinaryReader reader)
        {
            return new IntVec2(reader.ReadInt32(), reader.ReadInt32());
        }
        public static IntVec3 ReadIntVec3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        }
        public static T ReadIntVec3<T>(this T collection, IDataReader r)
           where T : ICollection<IntVec3>, new()
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                collection.Add(r.ReadIntVec3());
            return collection;
        }
        public static List<T> ReadList<T>(this IDataReader r) where T : ISerializableNew<T>
        {
            var count = r.ReadInt32();
            var list = new List<T>(count);
            for (int i = 0; i < count; i++)
                list.Add(T.Create(r));
            return list;
        }
        public static T[] ReadListAbstract<T>(this IDataReader r, params object[] constructorParams) where T : class, ISerializable
        {
            var count = r.ReadInt32();
            var list = new T[count];
            for (int i = 0; i < count; i++)
            {
                var typeName = r.ReadString();
                var instance = Activator.CreateInstance(Type.GetType(typeName), constructorParams) as T;
                list[i] = instance.Read(r) as T;
            }
            return list;
        }
        public static void ReadListAbstract<T>(this ICollection<T> list, IDataReader r, params object[] constructorParams) where T : class, ISerializable
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var typeName = r.ReadString();
                var instance = Activator.CreateInstance(Type.GetType(typeName), constructorParams) as T;
                list.Add(instance.Read(r) as T);
            }
        }
        public static List<int> ReadListInt(this BinaryReader r)
        {
            var count = r.ReadInt32();
            List<int> list = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(r.ReadInt32());
            }
            return list;
        }
        public static List<IntVec3> ReadListIntVec3(this BinaryReader r)
        {
            var count = r.ReadInt32();
            var list = new List<IntVec3>(count);
            for (int i = 0; i < count; i++)
                list.Add(r.ReadIntVec3());
            return list;
        }
        public static List<T> ReadListNew<T>(this IDataReader r) where T : class, ISerializableNew<T>, new()
        {
            var count = r.ReadInt32();
            var list = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                var instance = new T();
                list.Add(instance.Read(r));
            }
            return list;
        }
        public static List<string> ReadListString(this BinaryReader r)
        {
            var count = r.ReadInt32();
            List<string> list = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(r.ReadString());
            }
            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="net">pass the net peer to resolve targets initially</param>
        /// <returns></returns>
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
        public static List<Vector2> ReadListVector2(this BinaryReader r)
        {
            var count = r.ReadInt32();
            var list = new List<Vector2>(count);
            for (int i = 0; i < count; i++)
                list.Add(r.ReadVector2());
            return list;
        }
        public static List<Vector3> ReadListVector3(this BinaryReader r)
        {
            var count = r.ReadInt32();
            var list = new List<Vector3>(count);
            for (int i = 0; i < count; i++)
                list.Add(r.ReadVector3());
            return list;
        }
        /// <summary>
        /// the good one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="r"></param>
        public static void ReadMutable<T>(this ICollection<T> collection, IDataReader r)
            where T : class, ISerializable, new()
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                collection.Add(new T().Read(r) as T);
        }
        public static void ReadMutableNew<T>(this ICollection<T> collection, IDataReader r)
            where T : class, ISerializableNew<T>, new()
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                collection.Add(new T().Read(r));
        }
        public static Dictionary<T, U> ReadFromFlat<T, U>(this Dictionary<T, U> dic, IDataReader r, Func<IDataReader, T> keyReader, Func<IDataReader, U> valueReader)
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var key = keyReader(r);
                var value = valueReader(r);
                dic.Add(key, value);
            }
            return dic;
        }
        public static void WriteFlat<T, U>(this Dictionary<T, U> dic, IDataWriter w) where T : ISerializableNew<T> where U : ISerializableNew<U>
        {
            w.Write(dic.Count);
            foreach (var (key, value) in dic)
            {
                key.Write(w);
                value.Write(w);
            }
        }
        public static string[] ReadStringArray(this BinaryReader r)
        {
            var count = r.ReadInt32();
            string[] array = new string[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = r.ReadString();
            }
            return array;
        }
        public static T ReadTargets<T>(this T collection, IDataReader r)
           where T : ICollection<TargetArgs>, new()
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                collection.Add(TargetArgs.Read(null, r));
            return collection;
        }
        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }
        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
        public static T ReadVector3<T>(this T collection, BinaryReader r)
            where T : ICollection<Vector3>, new()
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                collection.Add(r.ReadVector3());
            return collection;
        }
        public static Vector3? ReadVector3Nullable(this BinaryReader reader)
        {
            if (!reader.ReadBoolean())
                return null;
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
        static public byte[] Serialize(this string text)
        {
            using var m = new MemoryStream();
            using (var str = new BinaryWriter(m))
                str.Write(text);
            return m.ToArray();
        }

        public static ICollection<T> Sync<T>(this ICollection<T> list, IDataWriter b) where T : ISyncable
        {
            foreach (var i in list)
                i.Sync(b);
            return list;
        }
        public static ICollection<T> Sync<T>(this ICollection<T> list, IDataReader b) where T : ISyncable
        {
            foreach (var i in list)
                i.Sync(b);
            return list;
        }

        public static Dictionary<int, U> Sync<U>(this Dictionary<int, U> dic, IDataWriter w) where U : ISerializable
        {
            foreach (var vk in dic)
            {
                w.Write(vk.Key);
                vk.Value.Write(w);
            }
            return dic;
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

        //public static Dictionary<int, U> Sync<U>(this Dictionary<int, U> dic, IDataReader r) where U : ISerializable
        //{
        //    for (int i = 0; i < dic.Count; i++)
        //    {
        //        dic[r.ReadInt32()].Read(r);
        //    }
        //    return dic;
        //}
        //public static Dictionary<T, U> Sync<T, U>(this Dictionary<T, U> dic, IDataReader r) where U : ISerializable where T : Def
        //{
        //    for (int i = 0; i < dic.Count; i++)
        //    {
        //        var def = Def.GetDef(r.ReadString()) as T;
        //        dic[def].Read(r);
        //    }
        //    return dic;
        //}
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
        public static ICollection<T> SyncOld<T>(this ICollection<T> collection, IDataReader r)
            where T : ISerializable
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                collection.ElementAt(i).Read(r);
            return collection;
        }
        public static void Write(this BinaryWriter w, ICollection<Vector3> list)
        {
            var count = list.Count;
            w.Write(count);
            foreach (var i in list)
                w.Write(i);
        }
        public static void Write(this ICollection<IntVec3> list, BinaryWriter w)
        {
            var count = list.Count;
            w.Write(count);
            foreach (var i in list)
                w.Write(i);
        }
        public static void Write(this ICollection<IntVec3> list, IDataWriter w)
        {
            var count = list.Count;
            w.Write(count);
            foreach (var i in list)
                w.Write(i);
        }
        public static void Write(this BinaryWriter w, ICollection<IntVec3> list)
        {
            var count = list.Count;
            w.Write(count);
            foreach (var i in list)
                w.Write(i);
        }
        public static void Write(this BinaryWriter w, ICollection<TargetArgs> list)
        {
            var count = list.Count;
            w.Write(count);
            foreach (var i in list)
                w.Write(i);
        }
        public static Dictionary<int, int> Write(this Dictionary<int, int> dic, BinaryWriter w)
        {
            w.Write(dic.Count);
            foreach (var vk in dic)
            {
                w.Write(vk.Key);
                w.Write(vk.Value);
            }
            return dic;
        }
        public static Dictionary<int, int> Write(this Dictionary<int, int> dic, IDataWriter w)
        {
            w.Write(dic.Count);
            foreach (var vk in dic)
            {
                w.Write(vk.Key);
                w.Write(vk.Value);
            }
            return dic;
        }

        public static void Write(this BinaryWriter w, string[] list)
        {
            var count = list?.Length ?? 0;
            w.Write(count);
            for (int i = 0; i < count; i++)
            {
                w.Write(list[i]);
            }
        }
        public static void Write(this BinaryWriter w, int[] list)
        {
            var count = list?.Length ?? 0;
            w.Write(count);
            for (int i = 0; i < count; i++)
            {
                w.Write(list[i]);
            }
        }
        public static void Write<T>(this IDataWriter w, ICollection<T> list) where T : ISerializable
        {
            var count = list.Count;
            w.Write(count);
            foreach (var i in list)
                i.Write(w);
        }
        public static void Write<T>(this ICollection<T> list, IDataWriter w) where T : ISerializable
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
        public static void Write<T>(this BinaryWriter w, T items) where T : ICollection<int>, new()
        {
            var count = items.Count;
            w.Write(count);
            foreach (var i in items)
                w.Write(i);
        }
        public static void Write(this BinaryWriter w, List<int> items)
        {
            w.Write(items.Count);
            foreach (var i in items)
                w.Write(i);
        }
        public static void Write(this BinaryWriter w, ICollection<int> items)
        {
            w.Write(items.Count);
            foreach (var i in items)
                w.Write(i);
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
        public static void Write(this BinaryWriter w, List<Vector2> list)
        {
            w.Write(list.Count);
            foreach (var g in list)
                w.Write(g);
        }
        public static void Write(this BinaryWriter w, List<Vector3> list)
        {
            w.Write(list.Count);
            foreach (var g in list)
                w.Write(g);
        }
        public static void Write(this BinaryWriter w, IEnumerable<Vector3> list)
        {
            w.Write(list.Count());
            foreach (var g in list)
                w.Write(g);
        }
        public static void Write(this BinaryWriter w, List<string> strings)
        {
            w.Write(strings.Count);
            foreach (var s in strings)
                w.Write(s);
        }
        public static void Write(this IDataWriter w, Progress progress)
        {
            progress.Write(w);
        }
        public static void Write(this BinaryWriter writer, Vector2 vec2)
        {
            writer.Write(vec2.X);
            writer.Write(vec2.Y);
        }
        public static void Write(this BinaryWriter writer, Vector3 vec3)
        {
            writer.Write(vec3.X);
            writer.Write(vec3.Y);
            writer.Write(vec3.Z);
        }
        public static void Write(this BinaryWriter writer, IntVec3 vec3)
        {
            writer.Write(vec3.X);
            writer.Write(vec3.Y);
            writer.Write(vec3.Z);
        }
        public static void Write(this BinaryWriter writer, Vector3? vec3)
        {
            var has = vec3.HasValue;
            writer.Write(has);
            if (!has)
                return;
            var val = vec3.Value;
            writer.Write(val.X);
            writer.Write(val.Y);
            writer.Write(val.Z);
        }
        public static void Write(this BinaryWriter w, TargetArgs target)
        {
            target.Write(w);
        }
        public static void Write(this BinaryWriter writer, PacketType packetType)
        {
            writer.Write((int)packetType);
        }
        //public static void Write(this BinaryWriter writer, ISerializableNew item)
        //{
        //    item.Write(writer);
        //}
        public static void Write(this BinaryWriter w, params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var argType = arg.GetType();
                if (argType == typeof(int))
                    w.Write((int)arg);
                else if (argType == typeof(float))
                    w.Write((float)arg);
                else if (argType == typeof(string))
                    w.Write((string)arg);
                else if (argType == typeof(byte))
                    w.Write((byte)arg);
                else if (argType == typeof(short))
                    w.Write((short)arg);
                else if (argType == typeof(bool))
                    w.Write((bool)arg);
                else if (argType == typeof(long))
                    w.Write((long)arg);
                else if (argType == typeof(uint))
                    w.Write((uint)arg);
                else if (argType == typeof(ushort))
                    w.Write((ushort)arg);
                else if (argType == typeof(ulong))
                    w.Write((ulong)arg);
                else if (argType == typeof(decimal))
                    w.Write((decimal)arg);
                else if (argType == typeof(IntVec3))
                    w.Write((IntVec3)arg);
                else if (arg is Def def)
                    def.Write(w);
                else if (arg is TargetArgs target)
                    target.Write(w);
                else
                    throw new ArgumentException();
            }
        }

        public static void WriteAbstract<T>(this ICollection<T> list, IDataWriter w) where T : ISerializable
        {
            var count = list.Count;
            w.Write(count);
            foreach (var i in list)
            {
                w.Write(i.GetType().FullName);
                i.Write(w);
            }
        }
        public static void WriteAbstractNew<T>(this ICollection<T> list, IDataWriter w) where T : ISerializableNew<T>
        {
            var count = list.Count;
            w.Write(count);
            foreach (var i in list)
            {
                w.Write(i.GetType().FullName);
                i.Write(w);
            }
        }
        public static void WriteASCII(this BinaryWriter writer, string text)
        {
            byte[] encoded = Encoding.ASCII.GetBytes(text);
            writer.Write(encoded.Length);
            writer.Write(encoded);
        }

        public static void WriteDefs<T>(this ICollection<T> list, IDataWriter w) where T : Def
        {
            var count = list.Count;
            w.Write(count);
            foreach (var i in list)
                i.Write(w);
        }
        public static ICollection<T> WriteImmutableNew<T>(this ICollection<T> list, IDataWriter b) where T : ISerializableNew<T>
        {
            foreach (var i in list)
                i.Write(b);
            return list;
        }
        public static void WriteNew<T>(this ICollection<T> list, IDataWriter w) where T : ISerializableNew<T>
        {
            var count = list.Count;
            w.Write(count);
            foreach (var i in list)
                i.Write(w);
        }

        public static void WriteNew<T>(this IDataWriter w, ICollection<T> list) where T : ISerializableNew<T>
        {
            var count = list.Count;
            w.Write(count);
            foreach (var i in list)
                i.Write(w);
        }
        public static Dictionary<T, U> WriteNew<T, U>(this Dictionary<T, U> dic, BinaryWriter w, Action<T> keyWriter, Action<U> valueWriter)
        {
            w.Write(dic.Count);
            foreach (var vk in dic)
            {
                keyWriter(vk.Key);
                valueWriter(vk.Value);
            }
            return dic;
        }
        public static Dictionary<T, U> WriteNew<T, U>(this Dictionary<T, U> dic, IDataWriter w, Action<T> keyWriter, Action<U> valueWriter)
        {
            w.Write(dic.Count);
            foreach (var vk in dic)
            {
                keyWriter(vk.Key);
                valueWriter(vk.Value);
            }
            return dic;
        }
    }
}
