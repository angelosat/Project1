using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    static public class CollectionExtensions
    {
        static public T SelectRandomWeighted<T>(this IEnumerable<T> collection, Random random, Func<T, int> weightGetter)
        {
            return collection.ToArray().SelectRandomWeighted(random, weightGetter);
        }
        static public T SelectRandomWeighted<T>(this ICollection<T> collection, Random random, Func<T, int> weightGetter)
        {
            return collection.ToArray().SelectRandomWeighted(random, weightGetter);
        }
        static public T SelectRandomWeighted<T>(this T[] list, Random random, Func<T, int> weightGetter)
        {
            int currentP = 0;
            var count = list.Length;
            var weights = new (T order, int prob)[count];
            for (int i = 0; i < count; i++)
            {
                var o = list[i];
                var p = 1 + weightGetter(o);
                currentP += p;
                weights[i] = (o, currentP);
            }
            var val = random.Next(currentP);
            for (int i = 0; i < count; i++)
            {
                var (order, prob) = weights[i];
                if (val <= prob)
                    return order;
            }
            throw new Exception();
        }
        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value, Func<TKey, TValue, TValue> updater)
        {
            if (dic.TryGetValue(key, out TValue existing))
                updater(key, existing);
            else
                dic.Add(key, value);
        }
        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value, Func<TValue, TValue> updater)
        {
            if (dic.TryGetValue(key, out TValue existing))
                dic[key] = updater(existing);
            else
                dic.Add(key, value);
        }
        public static void Update<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, Func<TValue, TValue> updater)
        {
            if (dic.TryGetValue(key, out TValue existing))
                dic[key] = updater(existing);
        }
        public static bool TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, Action<TValue> action)
        {
            if (dic.TryGetValue(key, out TValue existing))
            {
                action(existing);
                return true;
            }
            return false;
        }
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, Func<TValue> getter)
        {
            if (!dic.TryGetValue(key, out TValue item))
            {
                item = getter();
                dic[key] = item;
            }
            return item;
        }

        public static bool HasSingle<T>(this IEnumerable<T> sequence, out T value)
        {
            if (sequence is IList<T> list)
            {
                if (list.Count == 1)
                {
                    value = list[0];
                    return true;
                }
            }
            else
            {
                using (var iter = sequence.GetEnumerator())
                {
                    if (iter.MoveNext())
                    {
                        value = iter.Current;
                        if (!iter.MoveNext()) return true;
                    }
                }
            }

            value = default;
            return false;
        }
    }
}
