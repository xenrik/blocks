using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class VoxelGenerator : MonoBehaviour {

    public Mesh PerimiterMesh;

    public Text GenerateTotalText;
    public Text GeneratedCountText;
    public Text GeneratedPercentageText;

    public Text DestroyTotalText;
    public Text DestroyedCountText;
    public Text DestroyedPercentageText;

    private CollisionRecorder recorder;
    private GameObject voxelRoot;
    private GameObject perimiterGO;
    private int generatedCount;

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

        StartCoroutine(GenerateVoxels(5));
    }

    private IEnumerator GenerateVoxels(int scale) { 
        voxelRoot = new GameObject("VoxelRoot");
        Bounds meshBounds = PerimiterMesh.bounds;

        float totalCount = ((meshBounds.max.x - meshBounds.min.x) / scale) *
            ((meshBounds.max.y - meshBounds.min.y) / scale) *
            ((meshBounds.max.z - meshBounds.min.z) / scale);
        GenerateTotalText.text = string.Format("~ {0:F1}", totalCount);

        generatedCount = 0;
        float colliderSpace = 0.1f;
        Vector3 colliderSize = new Vector3(scale - colliderSpace, scale - colliderSpace, scale - colliderSpace);
        for (float x = meshBounds.min.x; x <= meshBounds.max.x; x += scale) {
            for (float y = meshBounds.min.y; y <= meshBounds.max.y; y += scale) {
                for (float z = meshBounds.min.z; z <= meshBounds.max.z; z += scale) {
                    GameObject voxel = new GameObject("voxel");
                    voxel.SetActive(false);
                    voxel.hideFlags = HideFlags.HideInHierarchy;

                    voxel.transform.parent = voxelRoot.transform;
                    voxel.transform.position = new Vector3(x, y, z);
                    voxel.transform.localScale = colliderSize;

                    Rigidbody rigidbody = voxel.AddComponent<Rigidbody>();
                    rigidbody.isKinematic = true;
                    rigidbody.useGravity = false;

                    BoxCollider collider = voxel.AddComponent<BoxCollider>();
                    collider.isTrigger = true;
                    //collider.size = colliderSize;

                    ++generatedCount;
                    if (generatedCount % 100 == 0) {
                        GeneratedCountText.text = generatedCount.ToString();
                        GeneratedPercentageText.text = string.Format("{0:F2}", Mathf.Min(100, (generatedCount / totalCount) * 100));
                        yield return null;
                    }
                }
            }
        }

        GeneratedCountText.text = generatedCount.ToString();
        GeneratedPercentageText.text = string.Format("{0:F2} %", Mathf.Min(100, (generatedCount / totalCount) * 100));
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
        yield return new WaitForFixedUpdate();

        float destroyTotal = generatedCount - recorder.Count();
        DestroyTotalText.text = (generatedCount - recorder.Count()).ToString();

        // Destroy objects on the root which have not been captured by the recorder
        int count = 0;
        foreach (GameObject child in voxelRoot.SafeChildren()) {
            if (recorder.HasCollision(child)) {
                continue;
            }

            Destroy(child);
            ++count;
            if (count % 100 == 0) {
                DestroyedCountText.text = count.ToString();
                DestroyedPercentageText.text = string.Format("{0:F2}", Mathf.Min(100, (count / destroyTotal) * 100));
                yield return null;
            }
        }

        DestroyedCountText.text = count.ToString();
        DestroyedPercentageText.text = string.Format("{0:F2} %", Mathf.Min(100, (count / destroyTotal) * 100));
        Debug.Log("Finished!");
    }
}
