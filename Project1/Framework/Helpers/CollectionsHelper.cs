using System;
using System.Collections.Generic;

namespace Project1.Core.Helpers
{
    public static class CollectionsHelper
    {
        public static void FromValues<TKey,TValue>(this Dictionary<TKey,TValue> dic, ICollection<TValue> list, Func<TValue, TKey> keySelector)
        {
            dic.Clear();
            foreach(var i in list)
                dic.Add(keySelector(i), i);
        
        }
        
        
    }
}
