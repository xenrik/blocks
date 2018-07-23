using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Voxels/Voxel Definition")]
public class VoxelDefinition : ScriptableObject {
    public int VoxelId;
    public string VoxelName;

    public GameObject VoxelPrefab;
}
