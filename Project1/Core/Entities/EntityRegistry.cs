using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Core.Simulation;

namespace Project1.Core.Entities
{
    internal class EntityRegistry : IEnumerable<Entity>, INotifyCollectionChanged, IReadOnlyDictionary<int, Entity>, IReadOnlyCollection<Entity>
    {
        readonly WorldBase World;
        readonly Dictionary<int, Entity> _inner = [];
        readonly ObservableCollection<Entity> _innerObservable = [];
        public readonly ReadOnlyObservableCollection<Entity> Entities;
        public EntityRegistry(WorldBase world)
        {
            this.World = world;
            this._innerObservable.CollectionChanged += (s, e) => CollectionChanged?.Invoke(this, e);
            this.Entities = new ReadOnlyObservableCollection<Entity>(this._innerObservable);
        }
        int _nextEntityId = 1;
        public bool Add(Entity entity)
        {
            if (this._inner.ContainsKey(entity.RefId)) throw new Exception("Attempted to register entity with duplicate Id");

            if (entity.RefId == 0)
                entity.RefId = _nextEntityId++;
            else
                _nextEntityId = Math.Max(_nextEntityId, entity.RefId + 1);

            this._inner.Add(entity.RefId, entity);
            this._innerObservable.Add(entity);

            entity.World = this.World;
            entity.Net = this.World.Net;

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
        public IEnumerable<Entity> GetEntities(IEnumerable<EntityRefId> netIds)
        {
            return (from o in this._inner where netIds.Contains(o.Key) select o.Value);
        }
        internal SaveTag Save(string tagName)
        {
            var entitiesList = new SaveTag(SaveTag.Types.List, "Registry", SaveTag.Types.Compound);
            foreach (var entity in this._inner.Values)
            {
                var entitytag = new SaveTag(SaveTag.Types.Compound, "", entity.SaveInternal());
                entitiesList.Add(entitytag);
            }
            return entitiesList;
        }
        internal void Load(SaveTag tag)
        {
            var list = tag.Value as List<SaveTag>;
            foreach (var entityTag in list)
            {
                var obj = GameObject.Load(entityTag, this.World) as Entity ?? throw new NullReferenceException();
                obj.World = this.World;
                this.Add(obj);
            }
        }
        internal void Write(IDataWriter w)
        {
            w.Write(this._inner.Count);
            foreach (var entity in this._inner.Values)
                entity.Write(w);
        }
        internal void Read(IDataReader r)
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                //var entity = GameObject.Create(r) as Entity;
                //entity.World = this.World;
                var entity = GameObject.Create(r, this.World);
                this.Add(entity);
            }
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
