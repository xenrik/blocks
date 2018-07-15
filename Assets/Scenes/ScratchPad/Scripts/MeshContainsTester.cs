using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshContainsTester : MonoBehaviour {

    public GameObject meshGO;
    public GameObject pointGO;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        MeshFilter meshFilter = meshGO.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;

        Vector3 point = pointGO.transform.position;
        Outline outline = pointGO.GetComponent<Outline>();

        if (mesh.Contains(point, meshGO.transform, true)) {
            outline.OutlineColor = Color.red; 
        } else {
            outline.OutlineColor = Color.green;
        }
    }
}
