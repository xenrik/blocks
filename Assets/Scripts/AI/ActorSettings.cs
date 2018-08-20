using UnityEngine;
using System.Collections;

public class ActorSettings : MonoBehaviour {

    public PIDController angularController = new PIDController(5.0f /*33.7766f*/, 0, 0.2553191f);
    public PIDController headingController = new PIDController(1.0f, 0, 0.06382979f);
    public PIDController thrustController = new PIDController(5.0f, 0, 0.1f);

    public float thrust = 10.0f;
    public float thrustClamp = 10.0f;

    public float angularThrust = 1.0f;
    public float angulatThrustClamp = 10.0f;

    public float debugRayMultiplier = 1;

    public float hull;
    public float shields;
    public float armour;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}