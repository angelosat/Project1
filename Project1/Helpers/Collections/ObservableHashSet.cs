using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;

namespace Start_a_Town_
{
    public class ObservableHashSet<T> : ICollection<T>, INotifyCollectionChanged
    {
        readonly HashSet<T> Inner = new();

        public int Count => ((ICollection<T>)this.Inner).Count;

        public bool IsReadOnly => ((ICollection<T>)this.Inner).IsReadOnly;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(T item)
        {
            if (this.Inner.Contains(item))
                return;
            ((ICollection<T>)this.Inner).Add(item);
            this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item));
        }

        public void Clear()
        {
            ((ICollection<T>)this.Inner).Clear();
            this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item)
        {
            return ((ICollection<T>)this.Inner).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)this.Inner).CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)this.Inner).GetEnumerator();
        }

        public bool Remove(T item)
        {
            var removed = ((ICollection<T>)this.Inner).Remove(item);
            if(removed)
                this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item));
            return removed;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Inner).GetEnumerator();
        }
    }
}
