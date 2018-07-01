using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PauseEditor : MonoBehaviour {
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Break();
        }
	}
}
