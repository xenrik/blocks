using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ThrusterAction : MonoBehaviour, ForceActuator {

    public string FireThrusterPropertyName;
    public Vector3 Force;

    private string[] keyNames;

    private GameObject root;
    private Rigidbody rootBody;

    private new ParticleSystem particleSystem;
    private ParticleSystem.EmissionModule emissionModule;
    private Block rootBlock;

    // Use this for initialization
    void Start () {
        PropertyHolder properties = GetComponent<PropertyHolder>();
        if (properties == null) {
            Debug.Log($"No properties: {gameObject}");
            this.enabled = false;
            return;
        }

        string keyNameProperty = properties[FireThrusterPropertyName];
        if (keyNameProperty == null) {
            Debug.Log($"No key bound to fire thrusters: {gameObject}");
        } else {
            keyNames = keyNameProperty.Split(',');
        }

        root = gameObject.GetRoot();
        rootBlock = root.GetComponent<Block>();
        rootBody = root.GetComponent<Rigidbody>();

        particleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
        emissionModule = particleSystem.emission;
        emissionModule.enabled = false;

        particleSystem.Play();

        // Attach to any AI
        foreach (BaseAI ai in GetComponentsInParent<BaseAI>()) {
            ai.AddForceActuator(this);
            Debug.Log("Added force actuator");
        }
        Debug.Log("Finished adding force actuator");
    }

    void FixedUpdate () {
        // Ignore if we are not the player
        if (!rootBlock.IsPlayer) {
            this.enabled = false;
            return;
        }

        bool keyDown = keyNames.Any(keyName => Input.GetKey(keyName));
		if (keyDown) {
            Vector3 directedForce = transform.rotation * Force;
            rootBody.AddForceAtPosition(directedForce, transform.position);

            emissionModule.enabled = true;
        } else {
            emissionModule.enabled = false;
        }
    }

    public void ApplyForce(Vector3 force) {
        Vector3 forward = transform.forward;
        float costheta = Vector3.Dot(forward, force) / (forward.magnitude * force.magnitude);
        float theta = Mathf.Acos(costheta);
        if (theta > Mathf.PI) {
            theta -= Mathf.PI;
        }

        if (theta > -Mathf.PI && theta < Mathf.PI) {
            emissionModule.enabled = true;
            Vector3 directedForce = transform.rotation * (Force * force.magnitude * (Mathf.Abs(theta) / Mathf.PI));
            rootBody.AddForceAtPosition(directedForce, transform.position);
        } else {
            emissionModule.enabled = false;
        }
    }

    public void ApplyTorque(Vector3 torque) {
        ApplyForce(torque);
    }
}
