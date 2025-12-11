using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class PriorityQueue<P, V>
    {
        private readonly SortedDictionary<P, Queue<V>> list = new();
        public void Enqueue(P priority, V value)
        {
            Queue<V> q;
            if (!list.TryGetValue(priority, out q))
            {
                q = new Queue<V>();
                list.Add(priority, q);
            }
            q.Enqueue(value);
        }
        public V Dequeue()
        {
            // will throw if there isn’t any first element!
            var pair = list.First();
            var v = pair.Value.Dequeue();
            if (pair.Value.Count == 0) // nothing left of the top priority.
                list.Remove(pair.Key);
            return v;
        }
        public bool TryDequeue(out V found)
        {
            // will throw if there isn’t any first element!
            if (list.Count == 0)
            { 
                found = default; 
                return false; 
            }
            var pair = list.First();
            var v = pair.Value.Dequeue();
            if (pair.Value.Count == 0) // nothing left of the top priority.
                list.Remove(pair.Key);
            found = v;
            return true;
        }
        public V Peek()
        {
            var pair = list.First();
            var v = pair.Value.Peek();
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
                foreach (KeyValuePair<P, Queue<V>> pair in list)
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
            Queue<V> newQueue = null;
            P foundkey = default;
            foreach (var p in list)
                if(p.Value.Contains(item))
                {
                    newQueue = new Queue<V>(p.Value.Except(new V[] { item }));
                    list[p.Key] = newQueue;
                    foundkey = p.Key;
                    break;
                }
            if (newQueue != null)
                if (newQueue.Count == 0)
                    list.Remove(foundkey);
            return false;
        }
    }
}
