using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotator : MonoBehaviour {

    public Vector3 rotationSpeed;
    public bool initialiseRandom;

    private Vector3 rotation;

	// Use this for initialization
	void Start () {
		if (initialiseRandom) {
            rotation.x = Random.value * 360;
            rotation.y = Random.value * 360;
            rotation.z = Random.value * 360;
        }
    }
	
	// Update is called once per frame
	void Update () {
        rotation.x += Time.deltaTime * rotationSpeed.x;
        rotation.y += Time.deltaTime * rotationSpeed.y;
        rotation.z += Time.deltaTime * rotationSpeed.z;

        gameObject.transform.localRotation = Quaternion.Euler(rotation);
	}
}
