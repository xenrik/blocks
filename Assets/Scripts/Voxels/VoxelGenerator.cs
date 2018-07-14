using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VoxelGenerator : MonoBehaviour {

    public Mesh PerimiterMesh;

    private CollisionRecorder recorder;
    private GameObject voxelRoot;
    private GameObject perimiterGO;

    // Use this for initialization
    void Start() {
        // Step 1 -- Create a GameObject with the perimiter mesh, attach a MeshCollider and a collision detection script
        perimiterGO = new GameObject("PerimiterFinder");
        perimiterGO.SetActive(false);

        MeshFilter meshFilter = perimiterGO.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = PerimiterMesh;

        perimiterGO.AddComponent<MeshCollider>();
        recorder = perimiterGO.AddComponent<CollisionRecorder>();

        // Step 2 -- Generate a sequence of 1x1 Cubes with colliders using the bounds of the mesh so we can identify the cubes that are within the perimiter mesh

        StartCoroutine(GenerateVoxels(10));
    }

    private IEnumerator GenerateVoxels(int scale) { 
        voxelRoot = new GameObject("VoxelRoot");
        Bounds meshBounds = PerimiterMesh.bounds;
        int count = 0;

        float voxelCount = ((meshBounds.max.x - meshBounds.min.x) / scale) *
            ((meshBounds.max.y - meshBounds.min.y) / scale) *
            ((meshBounds.max.z - meshBounds.min.z) / scale);
        Debug.Log($"Generating {voxelCount} voxels...");
        Debug.Log($"Bounds: {meshBounds.min}-{meshBounds.max}");

        float colliderSpace = 0.1f;
        Vector3 colliderSize = new Vector3(scale - colliderSpace, scale - colliderSpace, scale - colliderSpace);
        for (float x = meshBounds.min.x; x <= meshBounds.max.x; x += scale) {
            for (float y = meshBounds.min.y; y <= meshBounds.max.y; y += scale) {
                for (float z = meshBounds.min.z; z <= meshBounds.max.z; z += scale) {
                    GameObject voxel = new GameObject("voxel");
                    voxel.SetActive(false);

                    voxel.transform.parent = voxelRoot.transform;
                    voxel.transform.position = new Vector3(x, y, z);
                    voxel.transform.localScale = colliderSize;

                    Rigidbody rigidbody = voxel.AddComponent<Rigidbody>();
                    rigidbody.isKinematic = true;
                    rigidbody.useGravity = false;

                    BoxCollider collider = voxel.AddComponent<BoxCollider>();
                    collider.isTrigger = true;
                    //collider.size = colliderSize;

                    ++count;
                    if (count % 100 == 0) {
                        Debug.Log($"Generated {count} voxels...");
                        yield return null;
                    }
                }
            }
        }

        StartCoroutine(EnableVoxels());
    }

    private IEnumerator EnableVoxels() {
        yield return null;

        Debug.Log($"Enabling voxels...");
        foreach (GameObject child in voxelRoot.Children()) {
            child.SetActive(true);
        }

        perimiterGO.SetActive(true);
        StartCoroutine(DestroyVoxels());
    }

    private IEnumerator DestroyVoxels() {
        Debug.Log($"There are: {recorder.Count()} collisions");
        yield return new WaitForFixedUpdate();
        Debug.Log($"There are: {recorder.Count()} collisions");


        // Destroy objects on the root which have not been captured by the recorder
        int count = 0;
        foreach (GameObject child in voxelRoot.SafeChildren()) {
            if (recorder.HasCollision(child)) {
                continue;
            }

            Destroy(child);
            ++count;
            if (count % 100 == 0) {
                Debug.Log($"Destroyed {count} voxels...");
                yield return null;
            }
        }

        Debug.Log($"Destroyed {count} non-colliding voxels");
    }
}
