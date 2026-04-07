using Project1.Framework;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;

namespace Project1.Core.Entities;

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
        foreach (var component in this._innerList)
            component.Tick();
    }
    internal void TickOffMap()
    {
        foreach (var component in this._innerList)
            component.TickOffMap();
    }
    internal void Resolve()
    {
        foreach (var comp in this._innerList) comp.Resolve();
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
    public void Add(EntityCompDef compDef) => this.Add(compDef.CreateInstance());
    internal void Write(IDataWriter w)
    {
        foreach (var i in this._innerList)
            i.Write(w);
    }
    internal void Read(IDataReader r)
    {
        for (int i = 0; i < this._innerList.Count; i++)
            this._innerList[i].Read(r);
    }
    internal SaveTag Save(string tagName)
    {
        var compTag = new SaveTag(SaveTag.Types.Compound, tagName);
        foreach (var comp in this._inner.Values)
        {
            var compSave = comp.SaveAs(comp.CompDef.Name);
            if (compSave is not null)
                compTag.Add(compSave);
        }
        return compTag;
    }
    internal void Load(SaveTag tag)
    {
        var compData = tag.Value as Dictionary<string, SaveTag>;
        foreach (var comp in this._inner.Values)
        {
            //var data = compData[comp.CompDef.Name];
            if (compData.TryGetValue(comp.CompDef.Name, out var data))
                comp.Load(this._owner, data);
        }
    }
    public void CreateAndResolve(ItemDef def)
    {
        foreach (var compDef in def.CompDefs)
        {
            var comp = compDef.CreateInstance();
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
