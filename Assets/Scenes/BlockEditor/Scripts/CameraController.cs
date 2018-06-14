using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public enum Axis {
        X, Y, Z,
        InvertX, InvertY, InvertZ
    };

    public string MouseEnabledButton;
    public string MouseXAxis;    
    public string MouseYAxis;

    public string KeyboardXAxis;
    public string KeyboardYAxis;

    public Axis CameraXAxis = Axis.X;
    public Axis CameraYAxis = Axis.Y;

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
        if (mouseEnabled) {
            axisX = Input.GetAxis(MouseXAxis);
            axisY = Input.GetAxis(MouseYAxis);
        }

        if (axisX == 0 && axisY == 0) {
            return;
        }

        Vector3 p = camera.transform.position;
        switch (CameraXAxis) {
        case Axis.X: p.x += axisX; break;
        case Axis.Y: p.y += axisX; break;
        case Axis.Z: p.z += axisX; break;
        case Axis.InvertX: p.x -= axisX; break;
        case Axis.InvertY: p.y -= axisX; break;
        case Axis.InvertZ: p.z -= axisX; break;
        }
        switch (CameraYAxis) {
        case Axis.X: p.x += axisY; break;
        case Axis.Y: p.y += axisY; break;
        case Axis.Z: p.z += axisY; break;
        case Axis.InvertX: p.x -= axisY; break;
        case Axis.InvertY: p.y -= axisY; break;
        case Axis.InvertZ: p.z -= axisY; break;
        }

        camera.transform.position = p;
	}
}
