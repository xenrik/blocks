using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForceTester : MonoBehaviour {

    public Vector3 Force;
    public Transform Target;
    public Text text;

    private Rigidbody body;


	// Use this for initialization
	void Start () {
        body = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector3 delta = Target.position - transform.position;

        Vector3 velocity = body.velocity;
        Vector3 maxDeltaV = Force * Time.fixedDeltaTime / body.mass;

        Vector3 acceleration;
        if (velocity.sqrMagnitude == 0) {
            acceleration = Force;
        } else {
            acceleration = Vector3.ClampMagnitude(MoreMaths.Divide(-MoreMaths.Square(velocity), (2 * delta)), Force.magnitude);
        }

        body.AddForce(acceleration);

        text.text = $"Delta: {delta} - Velocity: {velocity} - Acceleration: {acceleration}";
    }
}
