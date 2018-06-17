using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotator : MonoBehaviour {

    public Vector3 RotationSpeed;
    public bool InitialiseRandom;

	// Use this for initialization
	void Start () {
		if (InitialiseRandom) {
            Vector3 rotation;
            rotation.x = Random.value * 360;
            rotation.y = Random.value * 360;
            rotation.z = Random.value * 360;

            gameObject.transform.localRotation = Quaternion.Euler(rotation);
        }
    }
	
	// Update is called once per frame
	void Update () {
        Quaternion rotation = gameObject.transform.localRotation;

        Vector3 rotationDelta;
        rotationDelta.x = Time.deltaTime * RotationSpeed.x;
        rotationDelta.y = Time.deltaTime * RotationSpeed.y;
        rotationDelta.z = Time.deltaTime * RotationSpeed.z;

        rotation *= Quaternion.Euler(rotationDelta);
        gameObject.transform.localRotation = rotation;
	}
}
