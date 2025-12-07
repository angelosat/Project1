using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class SaveTag
    {
        public enum Types : byte
        {
            End,
            Byte,
            Short,
            Int,
            Long,
            Float,
            Double,
            ByteArray,
            String,
            List, //the value of a list tag is always a list of unnamed tags of the same type
            Compound, //the value of a compound tag is always a list of tags that can be accessed by name (a dictionary?)
            UInt16,
            UInt32,
            Bool,
            Vector3,
            Reference
        }

        static readonly Dictionary<string, SaveTag> SaveReferences = new();
        static readonly Dictionary<string, ISaveable> LoadReferences = new();

        public byte Type;
        public string Name;
        public object Value;
        readonly byte ListType;

        #region Indexers
        public T GetValue<T>(string name)
        {
            return (T)this[name].Value;
        }
        public T TagValueOrDefault<T>(string name, T def)
        {
            if (!this.TryGetTag(name, out SaveTag tag))
                return def;
            else
                return (T)tag.Value;
        }
        public bool TryGetTag(string name, Action<SaveTag> action)
        {
            if (!this.TryGetTag(name, out var tag))
            {
                return false;
            }
            else
            {
                action(tag);
                return true;
            }
        }
        public bool TryGetTag<T>(string name, ref T output) where T : ISaveableNew
        {
            if (!this.TryGetTag(name, out var tag))
            {
                return false;
            }
            else
            {
                output = (T)T.Create(tag);
                return true;
            }
        }
        public bool TryLoadList<T>(string name, ref List<T> output) where T : ISaveableNew
        {
            if (!this.TryGetTag(name, out var tag))
            {
                return false;
            }
            else
            {
                //foreach (var i in tag.Value as List<SaveTag>)
                    //output.Add((T)T.Create(i));
                output.AddRange((tag.Value as List<SaveTag>).Select(i => (T)T.Create(i)));
                return true;
            }
        }
        public bool TryGetTagValue<TValue>(string name, ref TValue value)
        {
            if (!this.TryGetTag(name, out var tag))
            {
                return false;
            }
            else
            {
                value = (TValue)tag.Value;
                return true;
            }
        }
        public bool TryGetTagValueOrDefault<TValue>(string name, out TValue value)
        {
            if (!this.TryGetTag(name, out var tag))
            {
                value = default;
                return false;
            }
            else
            {
                value = (TValue)tag.Value;
                return true;
            }
        }
        public bool TryGetTagValue<TValue>(string name, Action<TValue> valueConvert)
        {
            if (!this.TryGetTag(name, out var tag))
            {
                return false;
            }
            else
            {
                valueConvert((TValue)tag.Value);
                return true;
            }
        }
        public bool TryGetTag(string name, out SaveTag tag)
        {
            if (this.Type != 10)
            {
                tag = null;
                return false;
            }
            return (this.Value as Dictionary<string, SaveTag>).TryGetValue(name, out tag);//
        }

        public SaveTag this[string name]
        {
            get
            {
                if (this.Type != 10)
                    return null;

                return (this.Value as Dictionary<string, SaveTag>)[name];
            }
        }
        #endregion

        public override string ToString()
        {
            return this.Type + ": " + this.Name + ": " + this.Value.ToString();
        }

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="value">If the type is compound, the value is dropped unless it's a list of tags. 
        /// If the type is list, then the value can either be a list of tags (from which the type of the list is derived from the first element), 
        /// or the type of the list.</param>
        public SaveTag(Types type, string name = "", object value = null) : this((byte)type, name, value) { }
        SaveTag(byte type, string name = "", object value = null)
        {
            switch (type)
            {
                case 9:
                    if (value is List<SaveTag>)
                    {
                        this.ListType = (value as List<SaveTag>)[0].Type;
                        this.Value = value;
                    }
                    else
                    {
                        this.ListType = (byte)value;
                        this.Value = new List<SaveTag>();
                    }
                    break;
                case 10:
                    if (value is Dictionary<string, SaveTag>)
                        this.Value = value;
                    else if (value is List<SaveTag>)
                    {
                        // for backwards compatibility
                        this.Value = (value as List<SaveTag>).ToDictionary(foo => foo.Name);
                    }
                    else
                    {
                        if (value != null)
                            Console.WriteLine(name + " value dropped");
                        this.Value = new Dictionary<string, SaveTag>();// { Value as Tag }; //when you create a compound tag with a value, it initializes a new list<tag> with the value as its first element
                    }
                    break;

                default:
                    this.Value = value;
                    break;
            }
            this.Type = type;
            this.Name = name;
        }
        #endregion

        public void WriteTo(BinaryWriter w)
        {
            w.Write(this.Type);
            w.Write(this.Name);
            this.Write(w);
        }
        public void WriteWithRefs(BinaryWriter w)
        {
            // write referencables
            w.Write(SaveReferences.Count);
            foreach (var i in SaveReferences)
            {
                i.Value.WriteTo(w);
            }
            SaveReferences.Clear();
            this.WriteTo(w);
        }
        void Write(BinaryWriter writer)
        {
            switch (this.Type)
            {
                case 0:
                    break;
                case 1:
                    writer.Write((byte)this.Value);
                    break;
                case 2:
                    writer.Write((short)this.Value);
                    break;
                case 3:
                    writer.Write((int)this.Value);
                    break;
                case 4:
                    writer.Write((long)this.Value);
                    break;
                case 5:
                    writer.Write((float)this.Value);
                    break;
                case 6:
                    writer.Write((double)this.Value);
                    break;
                case 7:
                    writer.Write(((byte[])this.Value).Length);
                    writer.Write((byte[])this.Value);
                    break;
                case 8:
                    writer.Write((string)this.Value);
                    break;
                case 9:
                    writer.Write(this.ListType);
                    writer.Write((this.Value as List<SaveTag>).Count);
                    foreach (SaveTag tag in (List<SaveTag>)this.Value)
                        tag.Write(writer);
                    break;
                case 10:
                    Dictionary<string, SaveTag> Tags = (Dictionary<string, SaveTag>)this.Value;
                    writer.Write(Tags.Count);
                    foreach (SaveTag tag in Tags.Values)
                    {
                        writer.Write(tag.Type);
                        writer.Write(tag.Name);
                        tag.Write(writer);
                    }
                    writer.Write((byte)0);
                    break;
                case 11:
                    writer.Write((ushort)this.Value);
                    break;
                case 12:
                    writer.Write((uint)this.Value);
                    break;
                case 13:
                    writer.Write((bool)this.Value);
                    break;
                case 14:
                    writer.Write((Vector3)this.Value);
                    break;
                case 15: // reference
                    writer.Write((string)this.Value);
                    break;
                default:
                    break;
            }
        }

        static object Read(BinaryReader reader, byte type)
        {
            switch (type)
            {
                case 0:
                    return null;
                case 1:
                    return reader.ReadByte();
                case 2:
                    return reader.ReadInt16();
                case 3:
                    return reader.ReadInt32();
                case 4:
                    return reader.ReadInt64();
                case 5:
                    return reader.ReadSingle();
                case 6:
                    return reader.ReadDouble();
                case 7:
                    int l = reader.ReadInt32();
                    return reader.ReadBytes(l);
                case 8:
                    return reader.ReadString();
                case 9:
                    byte listtype = reader.ReadByte();
                    int length = reader.ReadInt32();
                    List<SaveTag> list = new List<SaveTag>(length);
                    for (int i = 0; i < length; i++)
                    {
                        list.Add(new SaveTag(listtype, "", Read(reader, listtype)));
                    }
                    if (length > 0)
                        return list;
                    else
                        return listtype;
                case 10:
                    int capacity = reader.ReadInt32();
                    Dictionary<string, SaveTag> cmpdnlist = new Dictionary<string, SaveTag>(capacity + 1);
                    // initialize dictionary to capacity + 1 because we add an empty escape item in the dictionary at the end 
                    // can we avoid that? we already have the item count (capacity), so can we just iterate that many times? 
                    byte nexttype;
                    do
                    {
                        nexttype = reader.ReadByte();
                        string nextname = "";
                        if (nexttype != 0)
                            nextname = reader.ReadString();
                        var t = new SaveTag(nexttype, nextname, Read(reader, nexttype));
                        cmpdnlist.Add(nextname, t);
                    } while (nexttype > 0);
                    return cmpdnlist;
                case 11:
                    return reader.ReadUInt16();
                case 12:
                    return reader.ReadUInt32();
                case 13:
                    return reader.ReadBoolean();
                case 14:
                    return reader.ReadVector3();
                case 15: //references
                    return reader.ReadString();
            }
            return null;
        }
        public static SaveTag ReadWithRefs(BinaryReader reader)
        {
            LoadReferences.Clear();
            int refCount = reader.ReadInt32();
            for (int i = 0; i < refCount; i++)
            {
                var typeRef = reader.ReadByte();
                var nameRef = reader.ReadString();
                if (!LoadReferences.ContainsKey(nameRef))
                {
                    var tag = new SaveTag(typeRef, nameRef, Read(reader, typeRef));
                    var typeName = tag.GetValue<string>("TypeName");
                    var item = Activator.CreateInstance(System.Type.GetType(typeName)) as ISaveable;
                    LoadReferences[nameRef] = item.Load(tag["Data"]);
                }
            }
            var s = Read(reader);
            return s;
        }
        public static SaveTag Read(BinaryReader r)
        {
            byte type = r.ReadByte();
            string name = r.ReadString();
            return new SaveTag(type, name, Read(r, type));
        }
        public SaveTag Add(string name, int value)
        {
            this.Add(new SaveTag(SaveTag.Types.Int, name, value));
            return this;
        }
        public SaveTag Add(string name, string value)
        {
            this.Add(new SaveTag(SaveTag.Types.String, name, value));
            return this;
        }
        public void Add(SaveTag tag)
        {
            if (this.Type == 9)
                (this.Value as List<SaveTag>).Add(tag);
            else if (this.Type == 10)
                (this.Value as Dictionary<string, SaveTag>).Add(tag.Name, tag);
        }
        public void Add<T>(string name, T item) where T : ISaveableNew
        {
            var save = item.Save(name);
            this.Add(save);
        }
        public void Add<T>(string name, List<T> item) where T : ISaveableNew
        {
            var save = item.Save(name);
            this.Add(save);
        }
        public int LoadInt(string name)
        {
            return (int)this[name].Value;
        }
        public IntVec3 LoadIntVec3(string name)
        {
            var tag = this[name];
            return (Vector3)tag.Value; // it's vector3 internally
        }
        public IntVec3 LoadVector3(string name)
        {
            var tag = this[name];
            return tag.LoadVector3();
        }
        public Vector3 LoadVector3()
        {
            if (this.Value is Vector3)
                return (Vector3)this.Value;
            else
                return new Vector3((float)this["X"].Value, (float)this["Y"].Value, (float)this["Z"].Value);
        }
        public List<Vector3> LoadListVector3()
        {
            return new List<Vector3>().Load(this);
        }
        public List<int> LoadListInt(string name)
        {
            return new List<int>().Load(this[name]);
        }
        public Vector2 LoadVector2()
        {
            return new Vector2((float)this["X"].Value, (float)this["Y"].Value);
        }
        public Vector2 LoadVector2(string name)
        {
            var tag = this[name];
            return new Vector2((float)tag["X"].Value, (float)tag["Y"].Value);
        }
        public Color LoadColor()
        {
            Color c = new Color();
            c.R = this.GetValue<Byte>("R");
            c.G = this.GetValue<Byte>("G");
            c.B = this.GetValue<Byte>("B");
            c.A = this.GetValue<Byte>("A");
            return c;
        }

        private static string RegisterRef(ILoadReferencable reference)
        {
            var refID = reference.GetUniqueLoadID();
            if (!SaveReferences.ContainsKey(refID))
            {
                var reftag = new SaveTag(SaveTag.Types.Compound, refID);
                reftag.Add(reference.GetType().FullName.Save("TypeName"));
                reftag.Add(reference.Save("Data"));
                SaveReferences[refID] = reftag;
            }
            return refID;
        }

        public bool TrySaveRef(ILoadReferencable reference, string name)
        {
            if (reference == null)
                return false;
            this.Add(SaveRef(reference, name));
            return true;
        }
        public bool TrySaveRefs<T>(IList<T> refs, string name) where T : ILoadReferencable
        {
            if (refs == null)
                return false;
            var tag = new SaveTag(Types.List, name, Types.Reference);
            foreach (var item in refs)
            {
                var refID = RegisterRef(item);
                tag.Add(new SaveTag(SaveTag.Types.Reference, "", refID));
            }
            this.Add(tag);
            return true;
        }
        public static SaveTag SaveRef(ILoadReferencable reference, string name)
        {
            if (reference == null)
                throw new Exception();
            string refID = RegisterRef(reference);
            return new SaveTag(SaveTag.Types.Reference, name, refID);
        }
        public static SaveTag SaveRefs<T>(List<T> refs, string name) where T : ILoadReferencable
        {
            var tag = new SaveTag(Types.List, name, Types.Reference);
            foreach (var item in refs)
            {
                var refID = RegisterRef(item);
                tag.Add(new SaveTag(SaveTag.Types.Reference, "", refID));
            }
            return tag;
        }
        public T LoadRef<T>() where T : class, ISaveable
        {
            var refID = (string)this.Value;
            return LoadReferences[refID] as T;
        }
        public T LoadRef<T>(string name) where T : class, ISaveable
        {
            var refID = (string)this[name].Value;
            return LoadReferences[refID] as T;
        }
        public bool TryLoadRef<T>(string name, out T item) where T : class, ISaveable
        {
            if (this.TryGetTag(name, out var t))
            {
                var refID = (string)this[name].Value;
                item = LoadReferences[refID] as T;
                return true;
            }
            item = null;
            return false;
        }
        public List<T> LoadRefs<T>(string name) where T : class, ISaveable
        {
            var list = this[name].Value as List<SaveTag>;
            var itemList = new List<T>();
            foreach (var tag in list)
            {
                var refID = (string)tag.Value;
                itemList.Add(LoadReferences[refID] as T);
            }
            return itemList;
        }
        public bool TryLoadRefs<T>(string name, ref List<T> list) where T : class, ISaveable
        {
            if (this.TryGetTag(name, out var l))
            {
                var taglist = l.Value as List<SaveTag>;
                foreach (var tag in taglist)
                {
                    var refID = (string)tag.Value;
                    list.Add(LoadReferences[refID] as T);
                }
                return true;
            }
            return false;
        }
        public bool TryLoadRefs<T>(Collection<T> list, string name) where T : class, ISaveable
        {
            if (this.TryGetTag(name, out var l))
            {
                var taglist = l.Value as List<SaveTag>;
                foreach (var tag in taglist)
                {
                    var refID = (string)tag.Value;
                    list.Add(LoadReferences[refID] as T);
                }
                return true;
            }
            return false;
        }
    }
}
