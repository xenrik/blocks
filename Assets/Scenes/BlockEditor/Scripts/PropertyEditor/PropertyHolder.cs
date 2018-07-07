using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertyHolder : MonoBehaviour, IEnumerable<KeyValuePair<string, string>> {

    public string this[string key] {
        get {
            string value;
            if (properties.TryGetValue(key, out value)) {
                return value;
            } else {
                return null; 
            }
        }

        set {
            properties[key] = value;
        }
    }

    public int Count {
        get {
            return properties.Count;
        }
    }

    public IEnumerator<KeyValuePair<string,string>> GetEnumerator() {
        return properties.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    private Dictionary<string, string> properties;

    public PropertyHolder() {
        properties = new Dictionary<string, string>();
    }
}
