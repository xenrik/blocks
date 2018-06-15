﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public string MoveButton;
    public string MouseXAxis;    
    public string MouseYAxis;

    public string KeyboardXAxis;
    public string KeyboardYAxis;

    private Camera[] cameras;
    private Camera currentCamera;

	// Use this for initialization
	void Start () {
        cameras = GetComponentsInChildren<Camera>();
	}

    // Update is called once per frame
    void Update() {
        bool moveEnabled = Input.GetButton(MoveButton);
        if (moveEnabled) {
            if (currentCamera != null) {
                MoveCamera(currentCamera);
            } else {
                // Select a camera if we can
                foreach (Camera camera in cameras) {
                    if (camera.pixelRect.Contains(Input.mousePosition)) {
                        currentCamera = camera;

                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;

                        break;
                    }
                }
            }
        } else if (currentCamera != null) {
            currentCamera = null;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void MoveCamera(Camera camera) { 
        float axisX = Input.GetAxis(KeyboardXAxis) / 2;
        float axisY = Input.GetAxis(KeyboardYAxis) / 2;

        Debug.Log($"X: {axisX} - Y: {axisY}");

        float mouseX = Input.GetAxis(MouseXAxis);
        float mouseY = Input.GetAxis(MouseYAxis);

        Quaternion rot = camera.transform.localRotation;
        Vector3 pos = camera.transform.localPosition;

        if (Tags.FLIGHT_CAMERA.HasTag(camera)) { 
            rot *= Quaternion.Euler(-mouseY, mouseX, 0);
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
