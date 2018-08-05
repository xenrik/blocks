using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public class VoxelRenderer : MonoBehaviour {

    /** The map we are rendering */
    public TextAsset VoxelMapData;

    /** Progress monitor for when we initially create the mesh */
    public UIProgressMonitor ProgressMonitor;

    /** The deserialised map of voxels */
    public VoxelMap VoxelMap {
        get; private set;
    }

    /** The mesh(es) for the voxel map */
    public VoxelMesh VoxelMesh {
        get; private set;
    }

    private GameObject meshGameObject;

    private float halfScale;
    private Array directions;
    private Dictionary<int, Material> voxelMaterials;

    // Use this for initialization
    void Start() {
        VoxelMap = new VoxelMap();
        VoxelMap.FromJson(VoxelMapData.text);

        halfScale = VoxelMap.Scale / 2.0f;
        directions = System.Enum.GetValues(typeof(FaceDirection));

        StartCoroutine(BuildMesh());
    }

    public void AddVoxel(IntVector3 voxel, int voxelId) {

    }

    public void RemoveVoxel(IntVector3 voxel) {
        // Ignore if there is no voxel at that position
        if (VoxelMap[voxel] == 0) {
            return;
        }

        // First remove it from the map (capture the material first)
        Material material = GetMaterial(VoxelMap[voxel]);
        VoxelMap[voxel] = 0;

        // Remove faces for the voxel from the mesh
        foreach (FaceDirection dir in directions) {
            VoxelFace face = new VoxelFace(voxel, dir);
            VoxelMesh.Remove(face, material);
        }

        // Add faces for any adjacent voxels
        IntVector3 adjacentVoxel;
        foreach (FaceDirection dir in directions) {
            int adjacentVoxelId = GetAdjacentVoxel(VoxelMap, voxel, dir, out adjacentVoxel);
            if (adjacentVoxelId != 0) {
                VoxelFace face = new VoxelFace(adjacentVoxel, dir.GetOppositeFace(), halfScale);
                VoxelMesh.Add(face, GetMaterial(adjacentVoxelId));
            }
        }

        VoxelMesh.Update();
    }

    private IEnumerator BuildMesh() {
        Stopwatch timer = new Stopwatch();
        timer.Start();

        Stopwatch yieldTimer = new Stopwatch();
        yieldTimer.Start();

        Debug.Log("Building Mesh...");

        // Work our which faces we need. We do this by checking to see if a two voxels share and adjacent
        // side.

        ProgressMonitor.Begin("Building Mesh", VoxelMap.Size);

        VoxelMesh voxelMesh = new VoxelMesh();
        voxelMaterials = new Dictionary<int, Material>();
        foreach (var voxel in VoxelMap) {
            ProgressMonitor.Worked(1);
            if (NeedsYield(yieldTimer)) {
                yield return null;
            }

            if (voxel.Value == 0) {
                continue;
            }

            Material material = GetMaterial(voxel.Value);
            foreach (FaceDirection dir in directions) {
                if (GetAdjacentVoxel(VoxelMap, voxel.Key, dir) == 0) {
                    // Add the face
                    VoxelFace face = new VoxelFace(voxel.Key, dir, halfScale);
                    voxelMesh.Add(face, material);
                } // else don't need a face here
            }
        }

        // Update and parent the meshes
        voxelMesh.Update();
        foreach (var meshGO in voxelMesh.GetGameObjects()) {
            meshGO.transform.parent = gameObject.transform;
            meshGO.transform.localPosition = VoxelMap.Offset; // Not sure this is right...should it be scaled?
            meshGO.transform.localRotation = Quaternion.identity;
        }

        // We're done!
        ProgressMonitor.Finished();
        VoxelMesh = voxelMesh;

        Debug.Log($"Completed in {timer.ElapsedMilliseconds / 1000.0f:F2}s");
    }

    private Material GetMaterial(int voxelId) {
        Material material;
        if (!voxelMaterials.TryGetValue(voxelId, out material)) {
            VoxelDefinition voxelDef = VoxelRegistry.GetRegistry()[voxelId];
            GameObject voxelPrefab = voxelDef.VoxelPrefab;

            material = voxelPrefab.GetComponent<MeshRenderer>().sharedMaterial;
            voxelMaterials[voxelId] = material;
        }

        return material;
    }

    private int GetAdjacentVoxel(VoxelMap map, IntVector3 key, FaceDirection dir) {
        IntVector3 adjacent;
        return GetAdjacentVoxel(map, key, dir, out adjacent);
    }

    private int GetAdjacentVoxel(VoxelMap map, IntVector3 key, FaceDirection dir, out IntVector3 adjacentVoxel) {
        switch (dir) {
        case FaceDirection.TOP: adjacentVoxel = key.Translate(0, 1, 0); break;
        case FaceDirection.BOTTOM: adjacentVoxel = key.Translate(0, -1, 0); break;
        case FaceDirection.RIGHT: adjacentVoxel = key.Translate(1, 0, 0); break;
        case FaceDirection.LEFT: adjacentVoxel = key.Translate(-1, 0, 0); break;
        case FaceDirection.FRONT: adjacentVoxel = key.Translate(0, 0, 1); break;
        case FaceDirection.BACK: adjacentVoxel = key.Translate(0, 0, -1); break;

        default:
            adjacentVoxel = IntVector3.ZERO;
            return 0;
        }

        if (adjacentVoxel.x < 0 ||
            adjacentVoxel.y < 0 ||
            adjacentVoxel.z < 0 ||
            adjacentVoxel.x >= map.Columns ||
            adjacentVoxel.y >= map.Rows ||
            adjacentVoxel.z >= map.Pages) {
            return 0;
        } else {
            return VoxelMap[adjacentVoxel];
        }
    }

    private bool NeedsYield(Stopwatch lastYeild) {
        if (lastYeild.ElapsedMilliseconds > 100) {
            lastYeild.Restart();
            return true;
        } else {
            return false;
        }
    }
}
