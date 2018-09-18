using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ThrusterAction : MonoBehaviour, ForceActuator {

    public string FireThrusterPropertyName;
    public float Force;

    private string[] keyNames;

    private GameObject root;
    private Rigidbody rootBody;

    private new ParticleSystem particleSystem;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.MainModule mainModule;
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
        mainModule = particleSystem.main;
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
            Vector3 directedForce = transform.forward * Force;
            rootBody.AddForceAtPosition(directedForce, transform.position);
            Debug.DrawRay(transform.position, directedForce * 10, Color.green);

            emissionModule.enabled = true;
        } else {
            emissionModule.enabled = false;
        }
    }

    public void ApplyForce(Vector3 force) {
        Vector3 forward = transform.forward;

        // Find the angle between the target force and forward
        float costheta = Vector3.Dot(forward, force) / (forward.magnitude * force.magnitude);
        float theta = Mathf.Acos(costheta);
        if (theta > Mathf.PI) {
            theta -= Mathf.PI;
        }

        if (theta > -Mathf.PI && theta < Mathf.PI) {
            // Get the percentile between zero and PI
            float d = Mathf.Clamp(1 - (Mathf.Abs(theta) / Mathf.PI), 0, 1);

            // Normal distribution
            float n = MoreMaths.NormalDistribution(0.2f, 1, d) / 2.0f;
            float p = n;

            if (p > float.Epsilon) {
                Vector3 directedForce = forward * Force * p;
                rootBody.AddForceAtPosition(directedForce, transform.position);

                Debug.DrawRay(transform.position, forward * Force, Color.blue);
                Debug.DrawRay(transform.position, directedForce, Color.red);

                //Debug.DrawRay(transform.position, forward * 10, Color.magenta);

                emissionModule.enabled = true;
                mainModule.startLifetime = p;
            } else {
                emissionModule.enabled = true;
            }
        } else {
            emissionModule.enabled = false;
        }
    }

    public void ApplyTorque(Vector3 torque) {
        ApplyForce(torque);
    }
}
