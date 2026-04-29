using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project1.Framework.Helpers;

static public class CollectionExtensions
{
    extension<T>(IReadOnlyCollection<T> collection)
    {
        public T SelectRandomWeighted(Random random, Func<T, int> weightGetter)
        {
            var weighted = collection
                .Select(i => (item: i, weight: weightGetter(i)))
                .Where(i => i.weight > 0)
                .ToList();
            var sum = weighted.Sum(i => i.weight);
            var roll = random.Next(sum);
            foreach(var (item, weight) in weighted)
            {
                roll -= weight;
                if (roll <= 0)
                    return item;
            }
            throw new UnreachableException();
        }
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
