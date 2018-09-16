using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirectedThrusterTester : MonoBehaviour {

    public GameObject Thruster;
    public GameObject Heading;

    public Text AngleText;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 direction = Thruster.transform.forward;
        Vector3 force = Heading.transform.position - Thruster.transform.position;

        float costheta = Vector3.Dot(direction, force) / (direction.magnitude * force.magnitude);
        float theta = Mathf.Acos(costheta);
        if (theta > Mathf.PI) {
            theta -= Mathf.PI;
        }

        float degrees = theta * Mathf.Rad2Deg;
        float a = Mathf.Abs(theta);
        float p = a / Mathf.PI;
        float d = Mathf.Clamp(1 - p, 0, 1);

        float normal = MoreMaths.NormalDistribution(0.3f, 1, d) / (1.0f + (1.0f / 3.0f));

        AngleText.text = $"{degrees} - {d} - {normal}";

        Debug.DrawRay(transform.position, direction * 10, Color.blue);
        Debug.DrawRay(transform.position, direction * 10 * d, Color.red);
        Debug.DrawRay(transform.position, direction * 10 * normal, Color.green);
    }
}
