using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Project1.Framework.Helpers
{
    public class ObservableHashSet<T> : ObservableCollection<T>, ICollection<T>, INotifyCollectionChanged, IReadOnlySet<T>
    {
        readonly HashSet<T> Inner = [];

        //public int Count => ((ICollection<T>)this.Inner).Count;

        public bool IsReadOnly => ((ICollection<T>)this.Inner).IsReadOnly;

        //public event NotifyCollectionChangedEventHandler CollectionChanged;

        //public void Add(T item)
        //{
        //    if (this.Inner.Contains(item))
        //        return;
        //    ((ICollection<T>)this.Inner).Add(item);
        //    this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item));
        //}

        //public void Clear()
        //{
        //    ((ICollection<T>)this.Inner).Clear();
        //    this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
        //}

        //public bool Contains(T item)
        //{
        //    return ((ICollection<T>)this.Inner).Contains(item);
        //}

        //public void CopyTo(T[] array, int arrayIndex)
        //{
        //    ((ICollection<T>)this.Inner).CopyTo(array, arrayIndex);
        //}

        //public IEnumerator<T> GetEnumerator()
        //{
        //    return ((IEnumerable<T>)this.Inner).GetEnumerator();
        //}

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return this.Inner.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return this.Inner.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return this.Inner.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return this.Inner.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return this.Inner.Overlaps(other);
        }

        //public bool Remove(T item)
        //{
        //    var removed = ((ICollection<T>)this.Inner).Remove(item);
        //    if(removed)
        //        this.CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item));
        //    return removed;
        //}

        public bool SetEquals(IEnumerable<T> other)
        {
            return this.Inner.SetEquals(other);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Inner).GetEnumerator();
        }
    }
}
