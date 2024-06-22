using System;
using System.Collections.Generic;

namespace Core.Utils
{
    
    public static class Utils
    {
        public static V AddOrGet<K, V>(this Dictionary<K, V> dict, K k, Func<K, V> fact)
        {
            if (dict.TryGetValue(k, out var value)) return value;
            value = fact(k);
            dict.Add(k, value);
            return value;
        }
    }
    
}
