using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Start_a_Town_
{
    public abstract class Def : Inspectable// ILabeled, IInspectable
    {
        public string Name;
        string _label;
        public override string Label => this._label;
        static public Dictionary<string, Def> Database = new();
        public Def()
        {

        }
        protected Def(string name)
        {
            this._label = name;
            this.Name = $"{this.GetType().Name}:{name.Replace(" ", "")}";
            //this.Name.ToConsole();
        }
        /// <summary>
        /// TODO use attribute
        /// </summary>
        /// <param name="defOfType"></param>
        public static void Register(Type defOfType)
        {
            Register(defOfType.GetFields().Select(f => f.GetValue(null) as Def));
        }
        public static void Register(IEnumerable<Def> defs)
        {
            foreach (var d in defs)
                Register(d);
        }
        static public void Register(Def def)
        {
            if (def.Name.IsNullEmptyOrWhiteSpace())
                throw new Exception();
            if (Database.ContainsKey(def.Name))
                throw new Exception($"Def {def} already exists");
            Database.Add(def.Name, def);
        }

        static public Def GetDef(BinaryReader r)
        {
            return GetDef(r.ReadString());
        }
        static public Def GetDef(string defName)
        {
            if (defName == null || string.IsNullOrEmpty(defName) || string.IsNullOrWhiteSpace(defName))
                return null;
            if (Database.TryGetValue(defName, out var result))
                return result;
            Log.Warning($"def \"{defName}\" does not exist");
            return null;
        }

        static public T GetDef<T>(IDataReader r) where T : Def
        {
            return GetDef<T>(r.ReadString());
        }
        static public T GetDef<T>(string defName) where T : Def
        {
            if (TryGetDef<T>(defName) is not T def)
            {
                //throw new Exception($"def \"{defName}\" does not exist");
                Log.Warning($"def \"{defName}\" does not exist");
                return null;
            }
            return def;
        }
        static public T TryGetDef<T>(string defName) where T : Def
        {
            if (defName == null || string.IsNullOrEmpty(defName) || string.IsNullOrWhiteSpace(defName))
                throw new Exception();
            if (Database.TryGetValue(defName, out var result))
                return result as T;
            return null;
        }
        public override string ToString() => this.Name;
        public SaveTag Save(string name = "")
        {
            return this.Name.Save(name);
        }
        public void Save(SaveTag tag, string name = "")
        {
            tag.Add(this.Name.Save(name));
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.Name);
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.Name);
        }
        internal static IEnumerable<T> GetDefs<T>() where T: Def
        {
            return Database.Values.OfType<T>();
        }

    }
}
