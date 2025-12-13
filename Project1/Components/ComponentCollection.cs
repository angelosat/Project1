using System.Collections;
using System.Collections.Generic;

namespace Start_a_Town_.Components
{
    public class ComponentCollection : IDictionary<string, EntityComp>, IEnumerable<EntityComp>
    {
        readonly Dictionary<string, EntityComp> Inner = new();

        public ICollection<string> Keys => ((IDictionary<string, EntityComp>)this.Inner).Keys;

        public ICollection<EntityComp> Values => ((IDictionary<string, EntityComp>)this.Inner).Values;

        public int Count => ((ICollection<KeyValuePair<string, EntityComp>>)this.Inner).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, EntityComp>>)this.Inner).IsReadOnly;

        public EntityComp this[string key] { get => ((IDictionary<string, EntityComp>)this.Inner)[key]; set => ((IDictionary<string, EntityComp>)this.Inner)[key] = value; }

        public void Tick()
        {
            foreach (var component in this.Inner.Values)
                component.Tick();
        }

        public T GetComponent<T>(string name) where T : EntityComp
        {
            return (T)this.Inner[name];
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, EntityComp>)this.Inner).ContainsKey(key);
        }

        public void Add(string key, EntityComp value)
        {
            ((IDictionary<string, EntityComp>)this.Inner).Add(key, value);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, EntityComp>)this.Inner).Remove(key);
        }

        public bool TryGetValue(string key, out EntityComp value)
        {
            return ((IDictionary<string, EntityComp>)this.Inner).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, EntityComp> item)
        {
            ((ICollection<KeyValuePair<string, EntityComp>>)this.Inner).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<string, EntityComp>>)this.Inner).Clear();
        }

        public bool Contains(KeyValuePair<string, EntityComp> item)
        {
            return ((ICollection<KeyValuePair<string, EntityComp>>)this.Inner).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, EntityComp>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, EntityComp>>)this.Inner).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, EntityComp> item)
        {
            return ((ICollection<KeyValuePair<string, EntityComp>>)this.Inner).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, EntityComp>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, EntityComp>>)this.Inner).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Inner).GetEnumerator();
        }

        IEnumerator<EntityComp> IEnumerable<EntityComp>.GetEnumerator()
        {
            return this.Values.GetEnumerator();
        }
    }
}
