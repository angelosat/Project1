using System;
using System.Collections.Generic;

namespace Start_a_Town_.Components
{
    public class ComponentCollection : Inspectable
    {
        readonly Dictionary<Type, EntityComp> _inner = [];
        GameObject _owner;
        public IEnumerable<EntityComp> Values => this._inner.Values;


        public ComponentCollection(GameObject owner)
        {
            this._owner = owner;
        }
        internal void Tick()
        {
            foreach (var component in this._inner.Values)
                component.Tick();
        }
        internal void Resolve()
        {
            foreach (var comp in this._inner.Values) comp.Resolve();
        }
        public T GetComponent<T>() where T : EntityComp
        {
            //return (T)this._inner[typeof(T)];
            if (this._inner.TryGetValue(typeof(T), out var result)) return (T)result;
            return null;
        }
        public EntityComp GetComponent(Type type)
        {
            return this._inner[type];
        }
        public void Add(EntityComp comp)
        {
            //this._inner.Add(comp.GetType(), comp);

            // replace existing comps? or throw exception?
            this._inner[comp.GetType()] = comp;
            comp.Owner = this._owner;
        }
        internal void Write(IDataWriter w)
        {
            w.Write(this._inner.Count);
            foreach(var (key, value) in this._inner)
            {
                w.Write(key.FullName);
                value.Write(w);
            }
        }
        internal void Read(IDataReader r)
        {
            int compCount = r.ReadInt32();
            for (int i = 0; i < compCount; i++)
            {
                var compType = Type.GetType(r.ReadString());
                this._inner[compType].Read(r);
            }
        }
        internal SaveTag Save(string tagName)
        {
            var compTag = new SaveTag(SaveTag.Types.Compound, tagName);
            foreach (var comp in this._inner.Values)
            {
                var compSave = comp.SaveAs(comp.GetType().FullName);
                if (compSave is not null)
                    compTag.Add(compSave);
            }
            return compTag;
        }
        internal void Load(SaveTag tag)
        {
            var compData = tag.Value as Dictionary<string, SaveTag>;
            //foreach (var compTag in compData.Values)
            //{
                foreach(var (k, v) in this._inner)
                {
                    var data = compData[k.FullName];
                    v.Load(this._owner, data);
                }
            //}
        }
        public void CreateAndResolve(ItemDef def)
        {
            foreach (var compType in def.CompTypes)
            {
                var comp = (EntityComp)Activator.CreateInstance(compType);
                this.Add(comp);
            }
            this.ApplySpecs(def.Specs);
            this.Resolve();
        }
        //public void Init(ItemVariantDef def)
        //{
        //    foreach (var compType in def.BaseDef.CompTypes)
        //    {
        //        var comp = (EntityComp)Activator.CreateInstance(compType);
        //        this.Add(comp);
        //    }
        //    //foreach (var props in def.BaseDef.Specs)
        //    //{
        //    //    props.ApplyDefaults(this._inner[props.CompClass]);
        //    //}
        //    this.ApplySpecs(def.BaseDef.Specs);
        //    this.Resolve();
        //}
        public void ApplySpecs(IEnumerable<EntityComp.Spec> overrides)
        {
            foreach(var spec in overrides)
                spec.ApplyDefaults(this._inner[spec.CompClass]);
        }
        //internal bool TryGetComponent<T>(out T var) where T : EntityComp
        //{
        //    this._inner.TryGetValue(typeof(T), out EntityComp existing);
        //    var = existing as T;
        //    return var != null;
        //}
        public bool TryGetComponent<T>(out T comp) where T : EntityComp
        {
            var result = this._inner.TryGetValue(typeof(T), out EntityComp c);
            comp = (T)c;
            return result;
        }
        internal void Initialize()
        {
            foreach (var comp in this._inner.Values)
                comp.InitializeOnce();
        }

    }

    //public class ComponentCollection : IDictionary<string, EntityComp>, IEnumerable<EntityComp>
    //{
    //    readonly Dictionary<string, EntityComp> Inner = new();

    //    public ICollection<string> Keys => ((IDictionary<string, EntityComp>)this.Inner).Keys;

    //    public ICollection<EntityComp> Values => ((IDictionary<string, EntityComp>)this.Inner).Values;

    //    public int Count => ((ICollection<KeyValuePair<string, EntityComp>>)this.Inner).Count;

    //    public bool IsReadOnly => ((ICollection<KeyValuePair<string, EntityComp>>)this.Inner).IsReadOnly;

    //    public EntityComp this[string key] { get => ((IDictionary<string, EntityComp>)this.Inner)[key]; set => ((IDictionary<string, EntityComp>)this.Inner)[key] = value; }

    //    public void Tick()
    //    {
    //        foreach (var component in this.Inner.Values)
    //            component.Tick();
    //    }

    //    public T GetComponent<T>(string name) where T : EntityComp
    //    {
    //        return (T)this.Inner[name];
    //    }

    //    public bool ContainsKey(string key)
    //    {
    //        return ((IDictionary<string, EntityComp>)this.Inner).ContainsKey(key);
    //    }

    //    public void Add(string key, EntityComp value)
    //    {
    //        ((IDictionary<string, EntityComp>)this.Inner).Add(key, value);
    //    }

    //    public bool Remove(string key)
    //    {
    //        return ((IDictionary<string, EntityComp>)this.Inner).Remove(key);
    //    }

    //    public bool TryGetValue(string key, out EntityComp value)
    //    {
    //        return ((IDictionary<string, EntityComp>)this.Inner).TryGetValue(key, out value);
    //    }

    //    public void Add(KeyValuePair<string, EntityComp> item)
    //    {
    //        ((ICollection<KeyValuePair<string, EntityComp>>)this.Inner).Add(item);
    //    }

    //    public void Clear()
    //    {
    //        ((ICollection<KeyValuePair<string, EntityComp>>)this.Inner).Clear();
    //    }

    //    public bool Contains(KeyValuePair<string, EntityComp> item)
    //    {
    //        return ((ICollection<KeyValuePair<string, EntityComp>>)this.Inner).Contains(item);
    //    }

    //    public void CopyTo(KeyValuePair<string, EntityComp>[] array, int arrayIndex)
    //    {
    //        ((ICollection<KeyValuePair<string, EntityComp>>)this.Inner).CopyTo(array, arrayIndex);
    //    }

    //    public bool Remove(KeyValuePair<string, EntityComp> item)
    //    {
    //        return ((ICollection<KeyValuePair<string, EntityComp>>)this.Inner).Remove(item);
    //    }

    //    public IEnumerator<KeyValuePair<string, EntityComp>> GetEnumerator()
    //    {
    //        return ((IEnumerable<KeyValuePair<string, EntityComp>>)this.Inner).GetEnumerator();
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return ((IEnumerable)this.Inner).GetEnumerator();
    //    }

    //    IEnumerator<EntityComp> IEnumerable<EntityComp>.GetEnumerator()
    //    {
    //        return this.Values.GetEnumerator();
    //    }
    //}
}
