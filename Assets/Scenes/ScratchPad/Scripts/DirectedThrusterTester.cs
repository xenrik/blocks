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
        Vector3 forward = Thruster.transform.forward;
        Vector3 force = Heading.transform.position - Thruster.transform.position;

        float costheta = Vector3.Dot(forward, force) / (forward.magnitude * force.magnitude);
        float theta = Mathf.Acos(costheta);
        if (theta > Mathf.PI) {
            theta -= Mathf.PI;
        }

        float degrees = theta * Mathf.Rad2Deg;
        AngleText.text = $"{degrees}";
    }
}
