using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;

namespace Start_a_Town_
{
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged
    {
        readonly Dictionary<TKey, TValue> Dictionary = new();

        public TValue this[TKey key] { get => ((IDictionary<TKey, TValue>)this.Dictionary)[key]; set => ((IDictionary<TKey, TValue>)this.Dictionary)[key] = value; }

        public ICollection<TKey> Keys => ((IDictionary<TKey, TValue>)this.Dictionary).Keys;

        public ICollection<TValue> Values => ((IDictionary<TKey, TValue>)this.Dictionary).Values;

        public int Count => ((ICollection<KeyValuePair<TKey, TValue>>)this.Dictionary).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)this.Dictionary).IsReadOnly;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(TKey key, TValue value)
        {
            ((IDictionary<TKey, TValue>)this.Dictionary).Add(key, value);
            this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, key));
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)this.Dictionary).Add(item);
            this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item.Key));
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)this.Dictionary).Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)this.Dictionary).Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return ((IDictionary<TKey, TValue>)this.Dictionary).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)this.Dictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>)this.Dictionary).GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            var r = ((IDictionary<TKey, TValue>)this.Dictionary).Remove(key);
            this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, key));
            return r;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var r = ((ICollection<KeyValuePair<TKey, TValue>>)this.Dictionary).Remove(item);
            this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item.Key));
            return r;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return ((IDictionary<TKey, TValue>)this.Dictionary).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Dictionary).GetEnumerator();
        }
    }
}
