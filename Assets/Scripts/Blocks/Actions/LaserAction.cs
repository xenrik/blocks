using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaserAction : MonoBehaviour {

    public string FireLaserPropertyName;
    public GameObject LaserPrefab;

    private string[] keyNames;
    private GameObject laser;

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
	void Update () {
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

        RaycastHit hit = new RaycastHit();
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        Vector3 target;
        if (Physics.Raycast(ray, out hit) && !hit.collider.gameObject.SharesAncestry(gameObject)) {
            target = hit.collider.gameObject.transform.position;
        } else {
            Vector3 mousePosition = Input.mousePosition;
            //Vector3 forward10 = laser.transform.position - (laser.transform.rotation * (Vector3.forward * 10));
            mousePosition.z = 50;

            target = camera.ScreenToWorldPoint(mousePosition);
        }

        float length = (target - laser.transform.position).magnitude;
        Vector3 scale = laser.transform.localScale;
        scale.z = length;
        laser.transform.localScale = scale;

        laser.transform.LookAt(target);
    }
}
