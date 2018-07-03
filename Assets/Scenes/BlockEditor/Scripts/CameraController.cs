using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public string MoveButton;
    public string MouseXAxis;    
    public string MouseYAxis;

    public string KeyboardXAxis;
    public string KeyboardYAxis;

    public float KeyboardAxisMultipler;
    public float FlightAngleMultiplier;

    private Camera[] cameras;
    private Camera currentCamera;

	// Use this for initialization
	void Start () {
        cameras = FindObjectsOfType<Camera>().Where(camera => 
            Tags.FLIGHT_CAMERA.HasTag(camera) ||
            Tags.DRAG_CAMERA.HasTag(camera)).ToArray();
	}

    public void Update() {
        bool moveEnabled = Input.GetButton(MoveButton);
        if (moveEnabled) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (currentCamera != null) {
                MoveCamera(currentCamera);
            } else {
                // Select a camera if we can
                Vector3 mousePosition = Input.mousePosition;
                currentCamera = cameras.FirstOrDefault(camera => camera.pixelRect.Contains(mousePosition));
            }
        } else if (currentCamera != null) {
            currentCamera = null;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void MoveCamera(Camera camera) { 
        float axisX = Input.GetAxis(KeyboardXAxis) * KeyboardAxisMultipler;
        float axisY = Input.GetAxis(KeyboardYAxis) * KeyboardAxisMultipler;

        float mouseX = Input.GetAxis(MouseXAxis);
        float mouseY = Input.GetAxis(MouseYAxis);

        Quaternion rot = camera.transform.localRotation;
        Vector3 pos = camera.transform.localPosition;

        if (Tags.FLIGHT_CAMERA.HasTag(camera)) { 
            rot *= Quaternion.Euler(-mouseY * FlightAngleMultiplier, mouseX * FlightAngleMultiplier, 0);
            Vector3 rotEuler = rot.eulerAngles;
            rotEuler.z = 0;
            rot = Quaternion.Euler(rotEuler);

            pos += rot * new Vector3(axisX, 0, axisY);
        } else {
            pos += rot * new Vector3(axisX + mouseX, mouseY, axisY);
        }

        camera.transform.localPosition = pos;
        camera.transform.localRotation = rot;
    }
}
