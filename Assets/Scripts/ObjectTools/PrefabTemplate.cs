using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabTemplate : MonoBehaviour, ObjectTemplate {

    public GameObject Prefab;

    public GameObject CreateFromTemplate() {
        return Instantiate(Prefab);
    }
}
