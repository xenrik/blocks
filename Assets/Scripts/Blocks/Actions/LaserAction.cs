﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaserAction : MonoBehaviour {

    public string FireLaserPropertyName;
    public GameObject LaserPrefab;
    public GameObject CollisionPrefab;

    private string[] keyNames;
    private GameObject laser;
    private GameObject collision;

    // Use this for initialization
    void Start () {
        PropertyHolder properties = GetComponentInParent<PropertyHolder>();
        if (properties == null) {
            Debug.Log($"No properties: {gameObject}");
            this.enabled = false;
            return;
        }

        string keyNameProperty = properties[FireLaserPropertyName];
        if (keyNameProperty == null) {
            Debug.Log($"No key bound to fire laser: {gameObject}");
            this.enabled = false;
            return;
        } else {
            keyNames = keyNameProperty.Split(',');
        }
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        bool keyDown = keyNames.Any(keyName => Input.GetKey(keyName));
        if (keyDown && laser == null) {
            FireLaser();
        } else if (!keyDown && laser != null) {
            StopLaser();
        } else if (laser != null) {
            UpdateLaser();
        }
    }

    private void FireLaser() {
        laser = Instantiate(LaserPrefab);
        laser.transform.parent = gameObject.transform;
        laser.transform.localPosition = Vector3.zero;

        collision = Instantiate(CollisionPrefab);
        collision.SetActive(false);
    }

    private void StopLaser() {
        Destroy(laser);
        laser = null;
    }

    private void UpdateLaser() {
        Camera camera = CameraExtensions.FindCameraUnderMouse();
        if (camera == null) {
            return;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 50;

        Vector3 target = camera.ScreenToWorldPoint(mousePosition);
        RaycastHit hit = new RaycastHit();
        Ray ray = new Ray(laser.transform.position, target - laser.transform.position);
        if (Physics.Raycast(ray, out hit)) { 
            target = hit.point;

            collision.transform.position = target;
            collision.SetActive(true);
        } else {
            collision.SetActive(false);
        }

        float length = (target - laser.transform.position).magnitude;
        Vector3 scale = laser.transform.localScale;
        scale.z = length;

        laser.transform.localScale = scale;
        laser.transform.LookAt(target);


    }
}
