using System.Collections.Generic;

namespace hobd{

public static class StaticHelpers{

    public static bool TryGetValue2<K,V>(this IDictionary<K,V> d, K key, out V value, V defvalue)
    {
        if (d.ContainsKey(key))
        {
            value = d[key];
            return true;
        }else{
            value = defvalue;
            return false;
        }
    }

}


}