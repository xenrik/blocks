using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabTemplate : MonoBehaviour, ObjectTemplate {

    public GameObject prefab;

    public GameObject CreateFromTemplate() {
        return Instantiate(prefab);
    }
}
