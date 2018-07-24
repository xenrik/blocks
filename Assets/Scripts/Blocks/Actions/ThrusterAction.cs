using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrusterAction : MonoBehaviour {

    public string FireThrusterPropertyName;
    public Vector3 Force;

    private string keyName;

    private GameObject root;
    private Rigidbody rootBody;
    private ParticleSystem.EmissionModule emissionModule;

    // Use this for initialization
    void Start () {
        PropertyHolder properties = GetComponent<PropertyHolder>();
        if (properties == null) {
            Debug.Log($"No properties: {gameObject}");
        }

        keyName = properties[FireThrusterPropertyName];
        if (keyName == null) {
            Debug.Log($"No key bound to fire thrusters: {gameObject}");
        }

        root = gameObject.GetRoot();
        rootBody = root.GetComponent<Rigidbody>();
        emissionModule = gameObject.GetComponentInChildren<ParticleSystem>().emission;
        emissionModule.enabled = false;
	}
	
	void FixedUpdate () {
		if (Input.GetKey(keyName)) {
            Vector3 directedForce = transform.rotation * Force;
            rootBody.AddForceAtPosition(directedForce, transform.position);

            emissionModule.enabled = true;
        } else {
            emissionModule.enabled = false;
        }
	}
}
