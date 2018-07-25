using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ThrusterAction : MonoBehaviour {

    public string FireThrusterPropertyName;
    public Vector3 Force;

    private string[] keyNames;

    private GameObject root;
    private Rigidbody rootBody;

    private new ParticleSystem particleSystem;
    private ParticleSystem.EmissionModule emissionModule;

    // Use this for initialization
    void Start () {
        PropertyHolder properties = GetComponent<PropertyHolder>();
        if (properties == null) {
            Debug.Log($"No properties: {gameObject}");
        }

        string keyNameProperty = properties[FireThrusterPropertyName];
        if (keyNameProperty == null) {
            Debug.Log($"No key bound to fire thrusters: {gameObject}");
        } else {
            keyNames = keyNameProperty.Split(',');
        }

        root = gameObject.GetRoot();
        rootBody = root.GetComponent<Rigidbody>();

        particleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
        emissionModule = particleSystem.emission;
        emissionModule.enabled = false;

        particleSystem.Play();
	}
	
	void FixedUpdate () {
        bool keyDown = keyNames.Any(keyName => Input.GetKey(keyName));
		if (keyDown) {
            Vector3 directedForce = transform.rotation * Force;
            rootBody.AddForceAtPosition(directedForce, transform.position);

            emissionModule.enabled = true;
        } else {
            emissionModule.enabled = false;
        }
    }
}
