using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightCameraController : MonoBehaviour {
    public string MouseEnabledButton;
    public string MouseXAxis;    
    public string MouseYAxis;

    public string KeyboardXAxis;
    public string KeyboardYAxis;
    
    private new Camera camera;
    private bool wasOverCamera = false;

	// Use this for initialization
	void Start () {
        camera = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        // Ignore if we're not over this camera
	    if (!camera.pixelRect.Contains(Input.mousePosition)) {
            wasOverCamera = false;
            return;
        }

        if (!wasOverCamera) {
            Input.ResetInputAxes();
            
        }
        wasOverCamera = true;

        float axisX = Input.GetAxis(KeyboardXAxis);
        float axisY = Input.GetAxis(KeyboardYAxis);

        bool mouseEnabled = Input.GetButton(MouseEnabledButton);
        if (axisX == 0 && axisY == 0 && !mouseEnabled) {
            return;
        }

        float mouseX = Input.GetAxis(MouseXAxis);
        float mouseY = Input.GetAxis(MouseYAxis);

        Quaternion rot = camera.transform.localRotation;
        rot *= Quaternion.Euler(-mouseY, mouseX, 0);
        Vector3 rotEuler = rot.eulerAngles;
        rotEuler.z = 0;
        rot = Quaternion.Euler(rotEuler);

        Vector3 pos = camera.transform.localPosition;
        pos += rot * new Vector3(axisX, 0, axisY);

        camera.transform.localPosition = pos;
        camera.transform.localRotation = rot;
	}
}
