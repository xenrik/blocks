using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaserAction : MonoBehaviour {

    public string FireLaserPropertyName;
    public GameObject LaserPrefab;
    public GameObject CollisionPrefab;

    public GameObject HighlightBlockPrefab;

    private string[] keyNames;
    private GameObject laser;
    private GameObject collision;
    private GameObject highlight;
    private GameObject highlight2;

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
        ParticleSystem[] particles = collision.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles) {
            var emission = ps.emission;
            emission.enabled = false;
        }

        highlight = Instantiate(HighlightBlockPrefab);
        highlight.GetComponent<Outline>().OutlineColor = Color.red;

        highlight2 = Instantiate(HighlightBlockPrefab);
        highlight2.GetComponent<Outline>().OutlineColor = Color.blue;
    }

    private void StopLaser() {
        Destroy(laser);
        laser = null;

        Destroy(highlight);
        highlight = null;

        Destroy(highlight2);
        highlight2 = null;

        StartCoroutine(DestroyParticles(collision));
        collision = null;
    }

    private IEnumerator DestroyParticles(GameObject collision) {
        ParticleSystem[] particles = collision.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles) {
            var emission = ps.emission;
            emission.enabled = false;
        }

        bool loop = true;
        while (loop) {
            loop = false;
            foreach (var ps in particles) {
                if (ps.particleCount > 0) {
                    loop = true;
                    break;
                }
            }

            yield return null;
        }

        Destroy(collision);
    }

    private void UpdateLaser() {
        Camera camera = CameraExtensions.FindCameraUnderMouse();
        if (camera == null) {
            return;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 50;

        Vector3 target = camera.ScreenToWorldPoint(mousePosition);
        Vector3 delta = target - laser.transform.position;
        if (delta.magnitude > 30) {
            delta = delta.normalized * 30.0f;
            target = laser.transform.position + delta;
        }

        // First check if the mouse pointer is over a mesh
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        bool hit = false;
        VoxelRenderer voxelRenderer = null;
        if (Physics.Raycast(ray, out hitInfo, 50.0f)) {
            target = hitInfo.point;

            // Protect a second ray to see if the point we are aiming at would collide first
            ray = new Ray(laser.transform.position, target - laser.transform.position);
            if (Physics.Raycast(ray, out hitInfo, 50.0f)) {
                target = hitInfo.point;
            }

            voxelRenderer = hitInfo.collider.gameObject.GetComponentInParent<VoxelRenderer>();
            hit = true;
        } 

        if (!hit) {
            // If not protect a ray from the origin to the target
            ray = new Ray(laser.transform.position, delta);
            if (Physics.Raycast(ray, out hitInfo, delta.magnitude)) {
                target = hitInfo.point;
                hit = true;
            }
            highlight.SetActive(false);
        } else if (voxelRenderer != null) {

            VoxelMap voxelMap = voxelRenderer.VoxelMap;
            Vector3 localPosition = target - voxelRenderer.gameObject.transform.position;

            Vector3 point;
            point.x = localPosition.x / voxelMap.Scale - voxelMap.Offset.x;
            point.y = localPosition.y / voxelMap.Scale - voxelMap.Offset.y;
            point.z = localPosition.z / voxelMap.Scale - voxelMap.Offset.z;

            // Clamp to the bounds of the map
            int x = Mathf.Clamp(Mathf.RoundToInt(point.x), 0, voxelMap.Columns);
            int y = Mathf.Clamp(Mathf.RoundToInt(point.y), 0, voxelMap.Rows);
            int z = Mathf.Clamp(Mathf.RoundToInt(point.z), 0, voxelMap.Pages);

            Vector3 position2 = new Vector3(x, y, z);
            position2 *= voxelMap.Scale;
            position2 += voxelMap.Offset;
            position2 += voxelRenderer.gameObject.transform.position;
            highlight2.transform.position = position2;

            IntVector3 nearestVoxel;
            if (voxelMap.FindNearestVoxel(localPosition, out nearestVoxel)) {
                Debug.Log($"Nearest voxel: {nearestVoxel}");
                Vector3 position = nearestVoxel;
                position *= voxelMap.Scale;
                position += voxelMap.Offset;
                position += voxelRenderer.gameObject.transform.position;

                highlight.transform.position = position;
                highlight.SetActive(true);
            } else {
                Debug.Log("No voxel found?");
                highlight.SetActive(false);
            }
        }

        delta = target - laser.transform.position;
        Vector3 scale = laser.transform.localScale;
        scale.z = delta.magnitude;

        laser.transform.localScale = scale;
        laser.transform.LookAt(target);

        delta = delta.normalized * (delta.magnitude - 0.1f);

        collision.transform.position = laser.transform.position + delta;
        ParticleSystem[] particles = collision.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles) {
            var emission = ps.emission;
            emission.enabled = hit;
        }
    }
}
