using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DictionaryExtensions {
    /**
     * Get a value from a dictionary. If the key is not present in the dictionary return the default
     * value
     */
    public static TValue GetOrDefault<TKey,TValue>(this Dictionary<TKey,TValue> dictionary, TKey key, TValue defaultValue = default(TValue)) {
        TValue value;
        if (dictionary.TryGetValue(key, out value)) {
            return value;
        } else {
            return defaultValue;
        }
    }

    /**
     * Add a value into a dictionary if the key is not already present. It returns either the value
     * added to the dictionary, or the existing value if the key was already present
     */
    public static TValue AddIfAbsent<TKey,TValue>(this Dictionary<TKey,TValue> dictionary, TKey key, TValue value) {
        if (dictionary.ContainsKey(key)) {
            return dictionary[key];
        } else {
            dictionary[key] = value;
            return value;
        }
    }
}
