using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Xml.Linq;

namespace Start_a_Town_
{
    static class SaveHelper
    {
        public static bool TrySave(this ISaveable item, SaveTag save, string name)
        {
            if (item == null)
                return false;
            save.Add(item.Save(name));
            return true;
        }
        public static bool TryLoad(this ISaveable item, SaveTag save, string name)
        {
            if (item == null)
                throw new Exception();
            return save.TryGetTag(name, t => item.Load(t));
        }
        public static void TryLoad<T>(this SaveTag save, string name, out T item) where T : class, ISaveable, new()
        {
            item = save.TryGetTag(name, out var t) ? new T().Load(t) as T : null;
        }

        public static void Save<T, U>(this Dictionary<T, U> dic, SaveTag save, string name, SaveTag.Types keyType, Func<T, object> keySelector) where U : ISaveable
        {
            var tag = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Compound);
            foreach (var vk in dic)
            {
                var vkTag = new SaveTag(SaveTag.Types.Compound);
                vkTag.Add(new SaveTag(keyType, "Key", keySelector(vk.Key)));
                vkTag.Add(vk.Value.Save("Value"));
                tag.Add(vkTag);
            }
            save.Add(tag);
        }
        public static void SaveNew<T, U>(this Dictionary<T, U> dic, SaveTag save, string name) where T : ISaveable where U : ISaveable
        {
            var tag = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Compound);
            foreach (var vk in dic)
            {
                var vkTag = new SaveTag(SaveTag.Types.Compound);
                vkTag.Add(vk.Key.Save("Key"));
                vkTag.Add(vk.Value.Save("Value"));
                tag.Add(vkTag);
            }
            save.Add(tag);
        }
        public static Dictionary<T, U> TrySync<T, U>(this Dictionary<T, U> dic, SaveTag save, string name, Func<SaveTag, T> keySelector) where U : ISaveable
        {
            if (!save.TryGetTag(name, out SaveTag tag))
                return dic;
            var vkList = tag.Value as List<SaveTag>;
            foreach (var vk in vkList)
            {
                var key = keySelector(vk["Key"]);
                if (key is not null) // HACK will i ever need null keys?
                    dic[key].Load(vk["Value"]);
            }
            return dic;
        }
        [Obsolete]
        public static SaveTag SaveOld(this IEnumerable<ISaveable> items, string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            var iType = items.FirstOrDefault()?.GetType();
            if (iType == null)
                return tag;
            var typeTag = new SaveTag(SaveTag.Types.String, "Type", iType.FullName);
            var list = new SaveTag(SaveTag.Types.List, "List", SaveTag.Types.Compound);
            foreach (var item in items)
                list.Add(item.Save(""));
            tag.Add(typeTag);
            tag.Add(list);
            return tag;
        }

        /// <summary>
        /// the good one
        /// </summary>
        /// <param name="items"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SaveTag SaveNewBEST(this IEnumerable<ISaveable> items, string name)
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Compound);
            foreach (var item in items)
                list.Add(item.Save(""));
            return list;
        }
        public static bool TrySaveNewBEST(this IEnumerable<ISaveable> items, SaveTag tag, string name)
        {
            if (items == null)
                return false;
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Compound);
            foreach (var item in items)
                list.Add(item.Save(""));
            tag.Add(list);
            return true;
        }
        public static void SaveNewBEST(this IEnumerable<ISaveable> items, SaveTag save, string name)
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Compound);
            foreach (var item in items)
                list.Add(item.Save(""));
            save.Add(list);
        }
        public static void SaveAbstract(this IEnumerable<ISaveable> items, SaveTag save, string name)
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Compound);
            foreach (var item in items)
            {
                var itemTag = new SaveTag(SaveTag.Types.Compound);
                itemTag.Add(item.GetType().FullName.Save("Type"));
                itemTag.Add(item.Save("Data"));
                list.Add(itemTag);
            }
            save.Add(list);
        }
        public static void LoadFrom<T>(this ICollection<T> items, SaveTag save) where T : class, ISaveableNew
        {
            var list = save.Value as List<SaveTag>;
            for (int i = 0; i < list.Count; i++)
            {
                var tag = list[i];
                var item = (T)T.Create(tag);// ["Data"]);
                items.Add(item);
            }
        }
        public static void LoadVariableTypes<T>(this ICollection<T> items, SaveTag save, params object[] ctorArgs) where T : class, ISaveable
        {
            var list = save.Value as List<SaveTag>;
            for (int i = 0; i < list.Count; i++)
            {
                var tag = list[i];
                var typeName = tag.GetValue<string>("Type");
                var item = Activator.CreateInstance(Type.GetType(typeName), ctorArgs) as T;
                item.Load(tag["Data"]);
                items.Add(item);
            }
        }
        public static bool TryLoadByValueAbstractTypes<U, T>(this IDictionary<U, T> items, SaveTag save, string name, Func<T, U> keySelector, params object[] ctorArgs) where T : class, ISaveable
        {
            return save.TryGetTag(name, t =>
            {
                var list = t.Value as List<SaveTag>;
                for (int i = 0; i < list.Count; i++)
                {
                    var tag = list[i];
                    var typeName = tag.GetValue<string>("Type");
                    var item = Activator.CreateInstance(Type.GetType(typeName), ctorArgs) as T;
                    item.Load(tag["Data"]);
                    items.Add(keySelector(item), item);
                }
            });
        }
        public static void LoadAbstract<T>(this ICollection<T> items, SaveTag save, string name, params object[] ctorArgs) where T : class, ISaveable
        {
            var list = save[name].Value as List<SaveTag>;
            for (int i = 0; i < list.Count; i++)
            {
                var tag = list[i];
                var typeName = tag.GetValue<string>("Type");
                var item = Activator.CreateInstance(Type.GetType(typeName), ctorArgs) as T;
                item.Load(tag["Data"]);
                items.Add(item);
            }
        }
        public static bool TryLoadVariableTypes<T>(this ICollection<T> items, SaveTag save, string name, params object[] ctorArgs) where T : class, ISaveable
        {
            return save.TryGetTag(name, t => items.LoadVariableTypes(t, ctorArgs));
        }
        public static bool Sync<T>(this ICollection<T> items, SaveTag save, string name) where T : class, ISaveable, new()
        {
            return save.TryGetTag(name, t =>
            {
                var list = t.Value as List<SaveTag>;
                for (int i = 0; i < list.Count; i++)
                    items.ElementAt(i).Load(list[i]);
            });
        }
        /// <summary>
        /// the good one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="save"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool TryLoadMutable<T>(this ICollection<T> items, SaveTag save, string name) where T : class, ISaveable, new()
        {
            return save.TryGetTag(name, t =>
            {
                var list = t.Value as List<SaveTag>;
                for (int i = 0; i < list.Count; i++)
                    items.Add(new T().Load(list[i]) as T);
            });
        }
        public static bool TryLoadMutable<T>(this SaveTag save, string name, ref T[] array) where T : class, ISaveable, new()
        {
            var list = save.Value as List<SaveTag>;
            if (list == null)
            {
                return false;
            }
            var count = list.Count;
            array = new T[count];
            for (int i = 0; i < count; i++)
                array[i] = new T().Load(list[i]) as T;
            return true;
        }

        public static void SaveAsList<T>(this IEnumerable<T> items, SaveTag tag, string name) where T : ISaveable, INamed
        {
            var listTag = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Compound);
            foreach (var item in items)
                listTag.Add(item.Save());
            tag.Add(listTag);
        }

        public static bool TryLoadAsList<T, U>(this T collection, SaveTag save, string name)
            where T : ICollection<U>
            where U : class, ISaveable, new()
        {
            return save.TryGetTag(name, tag =>
            {
                var tags = tag.Value as List<SaveTag>;
                foreach (var t in tags)
                {
                    collection.Add(new U().Load(t) as U);
                }
            });
        }
        public static bool Load<U>(this ICollection<U> collection, SaveTag save, string name, params object[] constructorParams)
            where U : class, ISaveable
        {
            return save.TryGetTag(name, tag =>
            {
                var tags = tag.Value as List<SaveTag>;
                foreach (var t in tags)
                {
                    var instance = Activator.CreateInstance(typeof(U), constructorParams) as U;
                    collection.Add(instance.Load(t) as U);
                }
            });
        }
        public static bool TryLoadList<T, U>(this T collection, SaveTag save, string name, params object[] constructorParams)
            where T : ICollection<U>
            where U : class, ISaveable
        {
            return save.TryGetTag(name, tag =>
            {
                var tags = tag.Value as List<SaveTag>;
                foreach (var t in tags)
                {
                    var instance = Activator.CreateInstance(typeof(U), constructorParams) as U;
                    collection.Add(instance.Load(t) as U);
                }
            });
        }
        public static void Save(this string value, SaveTag save, string name)
        {
            save.Add(value.Save(name));
        }
        public static void Save(this double value, SaveTag save, string name)
        {
            save.Add(value.Save(name));
        }
        public static void Save(this int value, SaveTag save, string name)
        {
            save.Add(value.Save(name));
        }
        public static void Save(this byte value, SaveTag save, string name)
        {
            save.Add(value.Save(name));
        }
        public static void Save(this float value, SaveTag save, string name)
        {
            save.Add(value.Save(name));
        }
        public static void Save(this bool value, SaveTag save, string name)
        {
            save.Add(value.Save(name));
        }
        public static void Save(this ulong value, SaveTag save, string name)
        {
            save.Add(((double)value).Save(name)); // cast ulong to double for saving
        }
        public static bool TryLoad(this ref int value, SaveTag save, string name)
        {
            return save.TryGetTagValueOrDefault(name, out value);
        }
        public static bool TryLoad(this ref float value, SaveTag save, string name)
        {
            return save.TryGetTagValueOrDefault(name, out value);
        }
        public static void Save(this Vector3[] vectors, SaveTag save, string name)
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Vector3);
            for (int i = 0; i < vectors.Length; i++)
            {
                list.Add(new SaveTag(SaveTag.Types.Vector3, "", vectors[i]));
            }
            save.Add(list);
        }

        public static bool TryLoad(this SaveTag save, string name, ref Vector3[] vectors)
        {
            if (save.TryGetTag(name, out var t))
            {
                var positions = t.Value as List<SaveTag>;
                var count = positions.Count;
                vectors = new Vector3[count];
                for (int i = 0; i < count; i++)
                {
                    vectors[i] = (Vector3)positions[i].Value;
                }
                return true;
            }
            return false;
        }
        public static bool TryLoadCollection<T>(this SaveTag save, string name, ref T vectors) where T : ICollection<Vector3>, new()
        {
            if (save.TryGetTag(name, out var t))
            {
                var positions = t.Value as List<SaveTag>;
                var count = positions.Count;
                vectors = new T();
                for (int i = 0; i < count; i++)
                {
                    vectors.Add((Vector3)positions[i].Value);
                }
                return true;
            }
            return false;
        }

        public static bool TryLoad<T>(this T vectors, SaveTag save, string name) where T : ICollection<Vector3>
        {
            return save.TryGetTag(name, t =>
            {
                var positions = t.Value as List<SaveTag>;
                var count = positions.Count;
                for (int i = 0; i < count; i++)
                {
                    vectors.Add((Vector3)positions[i].Value);
                }
            });
        }
        public static void Save(this IntVec2 vec2, SaveTag save, string name)
        {
            ((Vector2)vec2).Save(save, name);
        }
        public static SaveTag Save(this IntVec2 vec2, string name)
        {
            return ((Vector2)vec2).Save(name);
        }
        public static void Save(this IntVec3 vec3, SaveTag save, string name)
        {
            save.Add(new SaveTag(SaveTag.Types.Vector3, name, (Vector3)vec3));
        }
        public static SaveTag Save(this IntVec3 vec3, string name)
        {
            return new SaveTag(SaveTag.Types.Vector3, name, (Vector3)vec3);
        }
        public static void Save(this ICollection<IntVec3> vectors, SaveTag save, string name)
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Vector3);
            foreach (var pos in vectors)
                list.Add(new SaveTag(SaveTag.Types.Vector3, "", (Vector3)pos));
            save.Add(list);
        }
        public static IntVec3[] LoadArrayIntVec3(this SaveTag tag, string name)
        {
            var positions = tag[name].Value as List<SaveTag>;
            var count = positions.Count;
            var array = new IntVec3[count];
            for (int i = 0; i < count; i++)
                array[i] = (Vector3)positions[i].Value;
            return array;
        }
        public static void Load(this ICollection<IntVec3> list, SaveTag save, string name)
        {
            save.TryGetTag(name, tag =>
            {
                list.Clear();
                var positions = tag.Value as List<SaveTag>;
                foreach (var pos in positions)
                    list.Add((Vector3)pos.Value);
            });
        }
        public static void Load(this ICollection<Vector3> list, SaveTag save, string name)
        {
            save.TryGetTag(name, tag =>
            {
                list.Clear();
                var positions = tag.Value as List<SaveTag>;
                foreach (var pos in positions)
                    list.Add((Vector3)pos.Value);
            });
        }
        public static bool TryLoad(this ICollection<IntVec3> list, SaveTag tag, string name)
        {
            list.Clear();
            if (!tag.TryGetTagValueOrDefault(name, out List<SaveTag> positions))
                return false;
            foreach (var pos in positions)
                list.Add((IntVec3)pos.Value);
            return true;
        }
        public static SaveTag Save(this ICollection<Vector3> vectors, string name)
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Vector3);
            foreach (var pos in vectors)
                list.Add(new SaveTag(SaveTag.Types.Vector3, "", pos));
            return list;
        }
        public static SaveTag Save(this ICollection<IntVec3> vectors, string name)
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Vector3);
            foreach (var pos in vectors)
                list.Add(new SaveTag(SaveTag.Types.Vector3, "", (Vector3)pos));
            return list;
        }
        public static void Save(this ICollection<Vector3> vectors, SaveTag save, string name)
        {
            save.Add(vectors.Save(name));
        }
        public static List<IntVec3> Load(this List<IntVec3> list, List<SaveTag> positions)
        {
            list.Clear();
            foreach (var pos in positions)
                list.Add((Vector3)pos.Value);
            return list;
        }
        public static List<Vector3> Load(this List<Vector3> list, List<SaveTag> positions)
        {
            list.Clear();
            foreach (var pos in positions)
                list.Add((Vector3)pos.Value);
            return list;
        }
        public static List<Vector3> Load(this List<Vector3> list, SaveTag tag)
        {
            list.Clear();
            var positions = tag.Value as List<SaveTag>;
            foreach (var pos in positions)
                list.Add((Vector3)pos.Value);
            return list;
        }
        public static List<IntVec3> Load(this List<IntVec3> list, SaveTag tag)
        {
            list.Clear();
            var positions = tag.Value as List<SaveTag>;
            foreach (var pos in positions)
                list.Add((Vector3)pos.Value);
            return list;
        }
        public static T LoadTargets<T>(this T list, SaveTag tag) where T : ICollection<TargetArgs>, new()
        {
            list.Clear();
            var positions = tag.Value as List<SaveTag>;
            foreach (var pos in positions)
                list.Add((TargetArgs)pos.Value);
            return list;
        }
        public static T LoadIntVecs<T>(this T list, SaveTag tag) where T : ICollection<IntVec3>, new()
        {
            list.Clear();
            var positions = tag.Value as List<SaveTag>;
            foreach (var pos in positions)
                list.Add((Vector3)pos.Value);
            return list;
        }
        public static T LoadVectors<T>(this T list, SaveTag tag) where T : ICollection<Vector3>, new()
        {
            list.Clear();
            var positions = tag.Value as List<SaveTag>;
            foreach (var pos in positions)
                list.Add((Vector3)pos.Value);
            return list;
        }
        public static List<string> LoadStringList(this SaveTag tag, string name)
        {
            var collection = new List<string>();
            var items = tag[name].Value as List<SaveTag>;
            foreach (var pos in items)
                collection.Add((string)pos.Value);
            return collection;
        }
        /// <summary>
        /// U mast have a constructor that accepts a savetag as the first parameter
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="collection"></param>
        /// <param name="save"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool TryLoad<U>(this ICollection<U> collection, SaveTag save, string name, params object[] ctorArgs)
            where U : class, ISaveable
        {
            return save.TryGetTag(name, t =>
            {
                var tags = t.Value as List<SaveTag>;
                var count = tags.Count;
                for (int i = 0; i < count; i++)
                    collection.Add(Activator.CreateInstance(typeof(U), new[] { tags[i] }.Concat(ctorArgs).ToArray()) as U);
            });
        }
        public static bool TryLoad(this ICollection<int> collection, SaveTag save, string name)
        {
            return save.TryGetTag(name, t =>
            {
                var tags = t.Value as List<SaveTag>;
                var count = tags.Count;
                for (int i = 0; i < count; i++)
                    collection.Add((int)tags[i].Value);
            });
        }
        public static bool TryLoad<T, U>(this T list, SaveTag tag, string name)
            where U : class, ISaveable, new()
            where T : ICollection<U>
        {
            return tag.TryGetTag(name, t =>
            {
                list.Clear();
                if (t.Value is List<SaveTag> positions)
                    foreach (var pos in positions)
                        list.Add(new U().Load(pos.Value as SaveTag) as U);
            });
        }
        public static ICollection<T> LoadNewNew<T>(this ICollection<T> list, SaveTag tag, string name)
           where T : ISaveable, new()
        {
            tag.TryGetTag(name, t =>
            {
                list.Clear();
                if (t.Value is List<SaveTag> tags)
                    foreach (var tag in tags)
                    {
                        var item = new T().Load(tag);
                        list.Add((T)item);
                    }
            });
            return list;
        }
        public static void LoadNewNew<T>(this ICollection<T> list, SaveTag t)
           where T : ISaveable, new()
        {
            list.Clear();
            if (t.Value is List<SaveTag> tags)
                foreach (var tag in tags)
                {
                    var item = new T().Load(tag);
                    list.Add((T)item);
                }

        }
       
        public static T LoadNew<T, U>(this T list, SaveTag tag, string name)
            where U : ISaveable, new()
            where T : ICollection<U>
        {
            tag.TryGetTag(name, t =>
            {
                list.Clear();
                if (t.Value is List<SaveTag> tags)
                    foreach (var tag in tags)
                    {
                        var item = new U().Load(tag);
                        list.Add((U)item);
                    }
            });
            return list;
        }
        public static T[] LoadList<T>(this SaveTag tag, string name)
            where T : class, ISaveable, new()
        {
            var list = tag[name].Value as List<SaveTag>;
            var count = list.Count;
            var array = new T[count];
            for (int i = 0; i < count; i++)
                array[i] = new T().Load(list[i]) as T;
            return array;

        }
        public static T[] LoadListNew<T>(this SaveTag tag, string name)
            where T : ISaveableNewNew<T>
        {
            var list = tag[name].Value as List<SaveTag>;
            var count = list.Count;
            var array = new T[count];
            for (int i = 0; i < count; i++)
                array[i] = T.Create(list[i]);
            return array;

        }
        public static T[] LoadList<T>(this SaveTag tag)
            where T : class, ISaveable, new()
        {
            var list = tag.Value as List<SaveTag>;
            var count = list.Count;
            var array = new T[count];
            for (int i = 0; i < count; i++)
                array[i] = new T().Load(list[i]) as T;
            return array;
        }
        public static List<T> LoadListNew<T>(this SaveTag tag) where T : ISaveable, new()
        {
            var list = new List<T>();
            list.LoadNewNew(tag);
            return list;
        }
        public static void Load<T>(this T[] array, SaveTag save) where T : ISaveable, INamed
        {
            for (int i = 0; i < array.Length; i++)
            {
                var sk = array[i];
                save.TryGetTag(sk.Name, t => sk.Load(t));
            }
        }
        public static void Load<T>(this IList<T> array, SaveTag save) where T : ISaveable, INamed
        {
            for (int i = 0; i < array.Count; i++)
            {
                var sk = array[i];
                save.TryGetTag(sk.Name, t => sk.Load(t));
            }
        }
        public static bool TryLoadImmutable<T>(this T[] array, SaveTag tag, string name) where T : ISaveable, INamed
        {
            return tag.TryGetTag(name, t => array.Load(t));
        }
        public static bool TryLoadImmutable<T>(this IList<T> array, SaveTag tag, string name) where T : ISaveable, INamed
        {
            return tag.TryGetTag(name, t => array.Load(t));
        }
        public static void Load<T>(this T list, SaveTag save, string name) where T : ICollection<int>
        {
            list.Clear();
            var taglist = save[name].Value as List<SaveTag>;
            foreach (var item in taglist)
                list.Add((int)item.Value);
        }

        [Obsolete]
        public static List<T> Load<T>(this List<T> list, SaveTag save) where T : class, ISaveable
        {
            if (!save.TryGetTagValueOrDefault<string>("Type", out string objTypeName))
                return list;
            var objType = Type.GetType(objTypeName);
            list.Clear();

            var tags = save["List"].Value as List<SaveTag>;
            foreach (var t in tags)
            {
                var obj = Activator.CreateInstance(objType) as T;
                obj.Load(t);
                list.Add(obj);
            }
            return list;
        }

        public static bool Load<T, U>(this Dictionary<T, U> dic, SaveTag save, string name, Func<U, T> keySelector, Action<U> initializer = null)
            where U : class, ISaveable, new()
        {
            var list = new List<U>();
            if (!list.TryLoadMutable(save, name))
                return false;
            foreach (var i in list)
            {
                dic[keySelector(i)] = i;
                initializer?.Invoke(i);
            }
            return true;
        }

        public static void Save(this ISaveable item, SaveTag saveTag, string name)
        {
            saveTag.Add(item.Save(name));
        }
        /// <summary>
        /// Immutable means that the collection is already initialized and populated and its items will be updated from the save tag (no new item instantiation)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="saveTag"></param>
        /// <param name="tagName"></param>
        public static void SaveImmutable<T>(this T[] array, SaveTag saveTag, string tagName) where T : ISaveable, INamed
        {
            var tag = new SaveTag(SaveTag.Types.Compound, tagName);
            for (int i = 0; i < array.Length; i++)
            {
                var sk = array[i];
                tag.Add(sk.Save(sk.Name));
            }
            saveTag.Add(tag);
        }
        public static void SaveImmutable<T>(this IList<T> array, SaveTag saveTag, string tagName) where T : ISaveable, INamed
        {
            var tag = new SaveTag(SaveTag.Types.Compound, tagName);
            for (int i = 0; i < array.Count; i++)
            {
                var sk = array[i];
                tag.Add(sk.Save(sk.Name));
            }
            saveTag.Add(tag);
        }
        public static void Save(this IEnumerable<int> ints, SaveTag save, string name)
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Int);
            foreach (var item in ints)
                list.Add(new SaveTag(SaveTag.Types.Int, "", item));
            save.Add(list);
        }
        public static SaveTag Save(this IEnumerable<int> ints, string name)
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Int);
            foreach (var item in ints)
                list.Add(new SaveTag(SaveTag.Types.Int, "", item));
            return list;
        }
        public static SaveTag Save<T>(this IEnumerable<T> saveables, string name = "") where T : ISaveable, new()
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Compound);
            foreach (var item in saveables)
                list.Add(item.Save());
            return list;
        }
        public static SaveTag Save<T>(this ICollection<T> list, string name) where T : ISaveableNew
        {
            var save = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Compound);
            foreach (var item in list)
                save.Add(item.Save());
            return save;
        }
        public static List<int> Load(this List<int> list, List<SaveTag> positions)
        {
            foreach (var pos in positions)
                list.Add((int)pos.Value);
            return list;
        }
        public static List<int> Load(this List<int> list, SaveTag tag)
        {
            var positions = tag.Value as List<SaveTag>;
            foreach (var pos in positions)
                list.Add((int)pos.Value);
            return list;
        }
        public static SaveTag Save(this ICollection<Def> defs, string name)
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.String);
            foreach (var item in defs)
                list.Add(item.Name.Save(""));
            return list;
        }
        public static void SaveDefs<T>(this ICollection<T> defs, SaveTag save, string name) where T : Def
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.String);
            foreach (var item in defs)
                list.Add(item.Name.Save(""));
            save.Add(list);
        }
        public static bool LoadDefs<T>(this ICollection<T> defs, SaveTag tag, string name) where T : Def
        {
            var list = tag[name].Value as List<SaveTag>;
            foreach (var item in list)
                defs.Add(Def.GetDef<T>((string)item.Value));
            return true;
        }
        public static bool TryLoadDefs<T>(this ICollection<T> defs, SaveTag tag, string name) where T : Def
        {
            if (!tag.TryGetTag(name, out var listTag))
                return false;
            var list = listTag.Value as List<SaveTag>;
            foreach (var item in list)
                defs.Add(Def.GetDef<T>((string)item.Value));
            return true;
        }
        public static bool TryLoad(this ICollection<Def> defs, SaveTag tag, string name)
        {
            if (!tag.TryGetTag(name, out var listTag))
                return false;
            var list = listTag.Value as List<SaveTag>;
            foreach (var item in list)
                defs.Add(Def.GetDef((string)item.Value));
            return true;
        }
        public static GameObject LoadObject(this SaveTag tag, string name)
        {
            return GameObject.Load(tag[name]);
        }
        public static T LoadDef<T>(this SaveTag tag, string name) where T : Def
        {
            return Def.GetDef<T>(tag[name].Value as string);
        }
        public static bool TryLoadDef<T>(this SaveTag tag, string name, ref T target) where T : Def
        {
            if (!tag.TryGetTag(name, out var deftag))
                return false;
            target = Def.GetDef<T>((string)deftag.Value);
            return true;
        }
        public static SaveTag Save(this IEnumerable<string> strings, string name)
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.String);
            foreach (var item in strings)
                list.Add(new SaveTag(SaveTag.Types.String, "", item));
            return list;
        }
        public static List<string> Load(this List<string> list, List<SaveTag> strings)
        {
            foreach (var s in strings)
                list.Add((string)s.Value);
            return list;
        }
        public static List<string> Load(this List<string> list, SaveTag save, string name)
        {
            var strings = save[name].Value as List<SaveTag>;
            foreach (var s in strings)
                list.Add((string)s.Value);
            return list;
        }

        public static List<SaveTag> SaveAsList(this Vector3 pos)
        {
            return new List<SaveTag>() {
                new SaveTag(SaveTag.Types.Float, "X", pos.X),
                new SaveTag(SaveTag.Types.Float, "Y", pos.Y),
                new SaveTag(SaveTag.Types.Float, "Z", pos.Z),
            };
        }
        [Obsolete]
        public static SaveTag SaveOld(this Vector3 pos, string name = "")
        {
            return new SaveTag(SaveTag.Types.Compound, name, new List<SaveTag>() {
                new SaveTag(SaveTag.Types.Float, "X", pos.X),
                new SaveTag(SaveTag.Types.Float, "Y", pos.Y),
                new SaveTag(SaveTag.Types.Float, "Z", pos.Z),
            });
        }
        public static void Save(this Vector3 pos, SaveTag save, string name = "")
        {
            save.Add(new(SaveTag.Types.Vector3, name, pos));
        }

        public static SaveTag Save(this Vector3 pos, string name = "")
        {
            return new(SaveTag.Types.Vector3, name, pos);
        }

        public static SaveTag Save(this Vector2 pos, string name)
        {
            return new SaveTag(SaveTag.Types.Compound, name, new List<SaveTag>() {
                new SaveTag(SaveTag.Types.Float, "X", pos.X),
                new SaveTag(SaveTag.Types.Float, "Y", pos.Y)
            });
        }
        public static void Save(this Vector2 pos, SaveTag tag, string name)
        {
            tag.Add(pos.Save(name));
        }

        public static SaveTag Save(this int value, string name)
        {
            return new SaveTag(SaveTag.Types.Int, name, value);
        }
        public static SaveTag Save(this double value, string name)
        {
            return new SaveTag(SaveTag.Types.Double, name, value);
        }
        public static SaveTag Save(this byte value, string name)
        {
            return new SaveTag(SaveTag.Types.Byte, name, value);
        }
        public static SaveTag Save(this string value, string name)
        {
            return new SaveTag(SaveTag.Types.String, name, value);
        }
        public static SaveTag Save(this float value, string name)
        {
            return new SaveTag(SaveTag.Types.Float, name, value);
        }
        public static SaveTag Save(this bool value, string name)
        {
            return new SaveTag(SaveTag.Types.Bool, name, value);
        }

        public static bool TryLoad(this ref bool value, SaveTag tag, string name)
        {
            return tag.TryGetTagValueOrDefault<bool>(name, out value);
        }

        public static bool TryLoadRefs<T>(this IList<T> list, SaveTag save, string name) where T : class, ISaveable
        {
            if (save.TryGetTag(name, out var l))
            {
                var taglist = l.Value as List<SaveTag>;
                foreach (var tag in taglist)
                {
                    list.Add(tag.LoadRef<T>());
                }
                return true;
            }
            return false;
        }
        public static bool TrySaveRefs<T>(this IList<T> refs, SaveTag save, string name) where T : ILoadReferencable
        {
            return save.TrySaveRefs(refs, name);
        }


        public static SaveTag Save(this List<TargetArgs> list, string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Compound);
            foreach (var i in list)
                tag.Add(i.Save());
            return tag;
        }
        public static void Load(this List<TargetArgs> list, SaveTag tag)
        {
            list.Clear();
            var tags = tag.Value as List<SaveTag>;
            foreach (var i in tags)
                list.Add(new TargetArgs(i));

        }

        public static SaveTag Save(this Dictionary<string, int> dic, string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(dic.Keys.Save("Keys"));
            tag.Add(dic.Values.Save("Values"));
            return tag;
        }
        public static Dictionary<string, int> Load(this Dictionary<string, int> dic, SaveTag save)
        {
            var keys = new List<string>().Load(save["Keys"].Value as List<SaveTag>);
            var values = new List<int>().Load(save["Values"].Value as List<SaveTag>);
            var tempdic = keys.Zip(values, (k, v) => new { k, v });
            foreach (var i in tempdic)
                dic.Add(i.k, i.v);
            return dic;
        }
        public static SaveTag Save<U, T>(this Dictionary<U, T> dic, string name, Func<U, SaveTag> keyFunc, Func<T, SaveTag> valueFunc)
        {
            //var tag = new SaveTag(SaveTag.Types.Compound, name);
            var tag = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Compound);

            foreach (var kv in dic)
            {
                var vkTag = new SaveTag(SaveTag.Types.Compound);
                var ktag = keyFunc(kv.Key);
                ktag.Name = "Key";
                var vtag = valueFunc(kv.Value);
                vtag.Name = "Value";
                vkTag.Add(ktag);
                vkTag.Add(vtag);
                tag.Add(vkTag);
            }
            return tag;
        }
        public static void LoadNewNewNew<U,T>(this Dictionary<U, T> dic, SaveTag tag, Func<SaveTag, U> keyFunc, Func<SaveTag, T> valueFunc)
        {
            var list = tag.Value as List<SaveTag>;
            foreach (var i in list)
            {
                var key = keyFunc(i["Key"]);
                var value = valueFunc(i["Value"]);
                dic.Add(key, value);
            }
        }
        public static void SaveZip<T>(this Dictionary<string, T> dic, SaveTag save, string name) where T : ISaveable, new()
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(dic.Keys.Save("Keys"));
            tag.Add(dic.Values.Save("Values"));
            //return tag;
            save.Add(tag);
        }
        public static Dictionary<string, T> LoadZip<T>(this Dictionary<string, T> dic, SaveTag save) where T : ISaveable, new()
        {
            var keys = new List<string>().Load(save["Keys"].Value as List<SaveTag>);
            var values = new List<T>();
            values.LoadNew<List<T>, T>(save, "Values");
            var tempdic = keys.Zip(values, (k, v) => new { k, v });
            foreach (var i in tempdic)
                dic.Add(i.k, i.v);
            return dic;
        }
    }
}
