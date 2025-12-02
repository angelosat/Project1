using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Start_a_Town_
{
    internal class EntityRegistry : IEnumerable<Entity>, INotifyCollectionChanged, IReadOnlyDictionary<int, Entity>, IReadOnlyCollection<Entity>
    {
        readonly Dictionary<int, Entity> _inner = [];
        readonly ObservableCollection<Entity> _innerObservable = [];
        public readonly ReadOnlyObservableCollection<Entity> Entities;
        public EntityRegistry()
        {
            this._innerObservable.CollectionChanged += (s, e) => CollectionChanged?.Invoke(this, e);
            this.Entities = new ReadOnlyObservableCollection<Entity>(this._innerObservable);
        }
        public bool Add(Entity entity)
        {
            if (this._inner.ContainsKey(entity.RefId)) throw new Exception("Attempted to register entity with duplicate Id");
            this._inner.Add(entity.RefId, entity);
            this._innerObservable.Add(entity);
            return true;
        }

        public bool Remove(int refId)
        {
            if(!this._inner.TryGetValue(refId, out var entity)) throw new Exception("Attempted to remove a non existent entity id");
            this._innerObservable.Remove(entity);
            return _inner.Remove(entity.RefId);
        }
        public IEnumerable<Entity> GetEntities(IEnumerable<int> netIds)
        {
            return (from o in this._inner where netIds.Contains(o.Key) select o.Value);
        }
        public Entity this[int key] => this._inner[key];

        public IEnumerable<int> Keys => this._inner.Keys;

        public IEnumerable<Entity> Values => this._inner.Values;

        public int Count => this._inner.Count;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public bool ContainsKey(int key) => this._inner.ContainsKey(key);

        public IEnumerator<Entity> GetEnumerator() => this._inner.Values.GetEnumerator();

        public bool TryGetValue(int key, out Entity value) => this._inner.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => this._inner.Values.GetEnumerator();

        IEnumerator<KeyValuePair<int, Entity>> IEnumerable<KeyValuePair<int, Entity>>.GetEnumerator() => this._inner.GetEnumerator();
    }
}
