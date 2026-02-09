using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Framework;
using Project1.Framework.Serialization;

namespace Project1.Core.Entities
{
    public class EntityCompCollection : Inspectable
    {
        readonly Dictionary<Type, EntityComp> _inner = [];
        readonly List<EntityComp> _innerList = [];
        readonly Entity _owner;
        public IEnumerable<EntityComp> Values => this._inner.Values;
        public EntityCompCollection(Entity owner)
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
            if (this._inner.TryGetValue(typeof(T), out var result)) return (T)result;
            return null;
        }
        public EntityComp GetComponent(Type type)
        {
            return this._inner[type];
        }
        public void Add(EntityComp comp)
        {
            this._inner[comp.GetType()] = comp;
            this._innerList.Add(comp);
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
            // HACK FOR MIGRATING AWAY FROM TYPE NAME LOOKUP
            // build short-name dictionary
            var compDataShort = compData.ToDictionary(
                kv => kv.Key.Split('.').Last(),
                kv => kv.Value
            );

            foreach (var (type, comp) in this._inner)
            {
                var shortName = type.Name; // runtime type
                if (compDataShort.TryGetValue(shortName, out var data))
                    comp.Load(this._owner, data);
            }
            return;
            foreach (var (k, v) in this._inner)
            {
                var data = compData[k.FullName];
                v.Load(this._owner, data);
            }
        }
        public void CreateAndResolve(ItemDef def)
        {
            foreach (var compType in def.CompTypes)
            {
                var comp = (EntityComp)Activator.CreateInstance(compType);
                comp.RuntimeIndex = this._inner.Count;
                this.Add(comp);
            }
            this.ApplySpecs(def.Specs);
            this.Resolve();
        }
        
        public void ApplySpecs(IEnumerable<EntityComp.Spec> overrides)
        {
            foreach(var spec in overrides)
                spec.ApplyDefaults(this._inner[spec.CompClass]);
        }
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

        internal void ResolveReferences()
        {
            foreach (var c in this._inner.Values)
                c.ResolveReferencesNew();
        }

        internal EntityComp GetComp(int compindex)
        {
            return this._innerList[compindex];
        }
    }
}
