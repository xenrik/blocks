using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMapCollider : MonoBehaviour {

    private VoxelMap voxelMap;

	// Use this for initialization
	void Start () {
        voxelMap = GetComponent<VoxelMap>();
        if (voxelMap == null) {
            Debug.Log("No voxel map found on game object");
            enabled = false;
        }
	}
	
}
