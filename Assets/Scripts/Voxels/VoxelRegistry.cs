using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Voxels/Voxel Registry")]
public class VoxelRegistry : ScriptableObject {
    /** Lazy loaded on first access */
    private static VoxelRegistry registry;

    public VoxelDefinition this[int voxelId] {
        get {
            VoxelDefinition result = null;
            if (voxelMap.TryGetValue(voxelId, out result)) {
                return result;
            } else {
                return null;
            }
        }
    }

    [SerializeField]
    private VoxelDefinition[] Voxels;

    private Dictionary<int, VoxelDefinition> voxelMap;

    public static VoxelRegistry GetRegistry() {
        if (registry == null) {
            registry = Resources.Load<VoxelRegistry>("VoxelRegistry");
            registry.Initialise();
            if (registry == null) {
                throw new System.InvalidOperationException("Could not load the voxel registry!");
            } else {
                Debug.Log("Voxel Registry Initialised:");
                foreach (VoxelDefinition def in registry.Voxels) {
                    Debug.Log($"   {def.VoxelId}");
                }
            }
        }

        return registry;
    }

    private void Initialise() {
        voxelMap = new Dictionary<int, VoxelDefinition>();

        foreach (var voxel in Voxels) {
            voxelMap[voxel.VoxelId] = voxel;
        }
    }
}
