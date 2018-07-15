using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Stopwatch = System.Diagnostics.Stopwatch;

public class VoxelGenerator2 : MonoBehaviour {

    public GameObject Perimiter;
    public Material Material;
    public int Scale;

    public Text GenerateTotalText;
    public Text GeneratedCountText;
    public Text GeneratedPercentageText;

    public Text DestroyTotalText;
    public Text DestroyedCountText;
    public Text DestroyedPercentageText;

    public Text TimeTakenText;

    private GameObject voxelRoot;
    private GameObject perimiterGO;
    private int generatedCount;

    // Use this for initialization
    void Start() {
        StartCoroutine(GenerateVoxels(Scale));
    }

    private IEnumerator GenerateVoxels(int scale) {
        Stopwatch timer = new Stopwatch();
        timer.Start();

        voxelRoot = new GameObject("VoxelRoot");
        Mesh perimiterMesh = Perimiter.GetComponent<MeshFilter>().sharedMesh;
        perimiterMesh = Perimiter.transform.TransformMesh(perimiterMesh);

        Bounds meshBounds = perimiterMesh.bounds;
        float totalCount = ((meshBounds.max.x - meshBounds.min.x) / scale) *
            ((meshBounds.max.y - meshBounds.min.y) / scale) *
            ((meshBounds.max.z - meshBounds.min.z) / scale);
        GenerateTotalText.text = string.Format("~ {0:F1}", totalCount);

        Stopwatch lastYield = new Stopwatch();
        lastYield.Start();

        int voxelCount = 0;
        generatedCount = 0;
        float colliderSpace = 0f;
        Vector3 colliderSize = new Vector3(scale - colliderSpace, scale - colliderSpace, scale - colliderSpace);
        Vector3 voxelOrigin;
        for (float x = meshBounds.min.x; x <= meshBounds.max.x; x += scale) {
            voxelOrigin.x = x;
            for (float y = meshBounds.min.y; y <= meshBounds.max.y; y += scale) {
                voxelOrigin.y = y;
                for (float z = meshBounds.min.z; z <= meshBounds.max.z; z += scale) {
                    voxelOrigin.z = z;
                    if (perimiterMesh.Contains(voxelOrigin)) {
                        //GameObject voxel = new GameObject("voxel");
                        GameObject voxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        voxel.hideFlags = HideFlags.HideInHierarchy;

                        voxel.transform.parent = voxelRoot.transform;
                        voxel.transform.position = voxelOrigin;
                        voxel.transform.localScale = colliderSize;
                        voxel.transform.rotation = Perimiter.transform.rotation;

                        if (Material != null) {
                            MeshRenderer renderer = voxel.GetComponent<MeshRenderer>();
                            renderer.material = Material;
                        }

                        ++voxelCount;
                    }

                    ++generatedCount;
                    if (NeedsYield(ref lastYield)) {
                        GeneratedCountText.text = generatedCount.ToString();
                        GeneratedPercentageText.text = string.Format("{0:F2}", Mathf.Min(100, (generatedCount / totalCount) * 100));

                        DestroyedCountText.text = voxelCount.ToString();
                        TimeTakenText.text = string.Format("{0:F2}s", timer.ElapsedMilliseconds / 1000.0f);
                        yield return null;
                    }
                }
            }
        }

        GeneratedCountText.text = generatedCount.ToString();
        GeneratedPercentageText.text = string.Format("{0:F2} %", Mathf.Min(100, (generatedCount / totalCount) * 100));
        TimeTakenText.text = string.Format("{0:F2}s", timer.ElapsedMilliseconds / 1000.0f);

        Debug.Log("Finished!");
    }

    private bool NeedsYield(ref Stopwatch lastYeild) {
        if (lastYeild.ElapsedMilliseconds > 100) {
            lastYeild.Restart();
            return true;
        } else {
            return false;
        }
    }
}
