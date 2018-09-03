using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraToggle : MonoBehaviour {

    private List<GameObject> cameraAnchors;
    private int currentAnchor = 0;

    private new Camera camera;

	// Use this for initialization
	void Start () {
        camera = GetComponentInChildren<Camera>();
        cameraAnchors = gameObject.ChildrenWithTag(Tags.CAMERA_ANCHOR).ToList();
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyDown(KeyCode.Tab)) {
            currentAnchor = (currentAnchor + 1) % cameraAnchors.Count;
            camera.transform.parent = cameraAnchors[currentAnchor].transform;

            camera.transform.localPosition = Vector3.zero;
            camera.transform.localRotation = Quaternion.identity;
        }
    }
}
