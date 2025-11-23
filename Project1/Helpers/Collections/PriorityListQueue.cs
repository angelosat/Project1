using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class PriorityListQueue<P, V>
    {
        private readonly SortedDictionary<P, List<V>> list = new();
        public void Enqueue(P priority, V value)
        {
            List<V> q;
            if (!list.TryGetValue(priority, out q))
            {
                q = new List<V>();
                list.Add(priority, q);
            }
            q.Add(value);
        }
        public V Dequeue()
        {
            // will throw if there isn’t any first element!
            var pair = list.First();
            var v = pair.Value[0];
            pair.Value.RemoveAt(0);
            if (pair.Value.Count == 0) // nothing left of the top priority.
                list.Remove(pair.Key);
            return v;
        }

        public V Peek()
        {
            var pair = list.First();
            var v = pair.Value[0];
            return v;
        }

        public void Clear()
        {
            list.Clear();
        }

        public int Count
        {
            get
            {
                int c = 0;
                foreach (KeyValuePair<P, List<V>> pair in list)
                    c += pair.Value.Count;
                return c;
            }
        }

        public bool IsEmpty
        {
            get { return !list.Any(); }
        }

        public bool Contains(V item)
        {
            foreach (var queue in list.Values)
                if (queue.Contains(item))
                    return true;
            return false;
        }
        public bool Any(Func<V, bool> predicate)
        {
            foreach (var queue in list.Values)
                if (queue.Any(predicate))
                    return true;
            return false;
        }
        public bool Remove(V item)
        {
            foreach (var p in list.Keys.ToList())
            {
                var listqueue = list[p];
                if (listqueue.Remove(item)) /// return when first item removed? or continue and remove equal items from all lists?
                {
                    list.Remove(p);
                    return true;
                }
            }
            return false;
        }
    }
}
