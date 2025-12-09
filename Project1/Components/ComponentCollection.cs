using System.Collections;
using System.Collections.Generic;

namespace Start_a_Town_.Components
{
    public class ComponentCollection : IDictionary<string, EntityComponent>, IEnumerable<EntityComponent>
    {
        readonly Dictionary<string, EntityComponent> Inner = new();

        public ICollection<string> Keys => ((IDictionary<string, EntityComponent>)this.Inner).Keys;

        public ICollection<EntityComponent> Values => ((IDictionary<string, EntityComponent>)this.Inner).Values;

        public int Count => ((ICollection<KeyValuePair<string, EntityComponent>>)this.Inner).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, EntityComponent>>)this.Inner).IsReadOnly;

        public EntityComponent this[string key] { get => ((IDictionary<string, EntityComponent>)this.Inner)[key]; set => ((IDictionary<string, EntityComponent>)this.Inner)[key] = value; }

        public void Tick()
        {
            foreach (var component in this.Inner.Values)
                component.Tick();
        }

        public T GetComponent<T>(string name) where T : EntityComponent
        {
            return (T)this.Inner[name];
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, EntityComponent>)this.Inner).ContainsKey(key);
        }

        public void Add(string key, EntityComponent value)
        {
            ((IDictionary<string, EntityComponent>)this.Inner).Add(key, value);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, EntityComponent>)this.Inner).Remove(key);
        }

        public bool TryGetValue(string key, out EntityComponent value)
        {
            return ((IDictionary<string, EntityComponent>)this.Inner).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, EntityComponent> item)
        {
            ((ICollection<KeyValuePair<string, EntityComponent>>)this.Inner).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<string, EntityComponent>>)this.Inner).Clear();
        }

        public bool Contains(KeyValuePair<string, EntityComponent> item)
        {
            return ((ICollection<KeyValuePair<string, EntityComponent>>)this.Inner).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, EntityComponent>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, EntityComponent>>)this.Inner).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, EntityComponent> item)
        {
            return ((ICollection<KeyValuePair<string, EntityComponent>>)this.Inner).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, EntityComponent>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, EntityComponent>>)this.Inner).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Inner).GetEnumerator();
        }

        IEnumerator<EntityComponent> IEnumerable<EntityComponent>.GetEnumerator()
        {
            return this.Values.GetEnumerator();
        }
    }
}
