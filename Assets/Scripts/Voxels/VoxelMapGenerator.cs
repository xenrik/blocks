using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;
using Stopwatch = System.Diagnostics.Stopwatch;

public class VoxelMapGenerator : MonoBehaviour {

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

    private int generatedCount;

    private BlockingCollection<Vector3> voxelsToGenerate = new BlockingCollection<Vector3>();
    private BlockingCollection<Vector3> voxelsToCheck = new BlockingCollection<Vector3>();

    // Use this for initialization
    void Start() {
        StartCoroutine(GenerateVoxels(Scale));
    }

    private IEnumerator GenerateVoxels(int scale) {
        Stopwatch timer = new Stopwatch();
        timer.Start();

        VoxelMap voxelMap = new VoxelMap();

        Mesh perimiterMesh = Perimiter.GetComponent<MeshFilter>().sharedMesh;
        perimiterMesh = Perimiter.transform.ScaleMesh(perimiterMesh);

        Bounds meshBounds = perimiterMesh.bounds;
        float totalCount = ((meshBounds.max.x - meshBounds.min.x) / scale) *
            ((meshBounds.max.y - meshBounds.min.y) / scale) *
            ((meshBounds.max.z - meshBounds.min.z) / scale);
        GenerateTotalText.text = string.Format("~ {0:F1}", totalCount);

        int voxelCount = 0;
        generatedCount = 0;
        float colliderSpace = 0f;
        Vector3 colliderSize = new Vector3(scale - colliderSpace, scale - colliderSpace, scale - colliderSpace);
        Vector3 voxelOrigin;

        Bounds bounds = perimiterMesh.bounds;
        int[] triangles = perimiterMesh.triangles;
        Vector3[] vertices = perimiterMesh.vertices;
        List<Task> consumers = new List<Task>();
        for (int i = 0; i < 5; ++i) {
            consumers.Add(Task.Factory.StartNew(() => {
                CheckVoxels(bounds, triangles, vertices);
            }));
        }

        Vector3 min = meshBounds.min;
        min.x += scale / 2.0f; min.y += scale / 2.0f; min.z += scale / 2.0f;

        Vector3 max = meshBounds.max;
        max.x -= scale / 2.0f; max.y -= scale / 2.0f; max.z -= scale / 2.0f;
        Debug.Log($"Generating voxels to check (bounds: {min}-{max})...");

        Stopwatch lastYield = new Stopwatch();
        lastYield.Start();
        for (float x = min.x; x <= max.x; x += scale) {
            voxelOrigin.x = x;
            for (float y = min.y; y <= max.y; y += scale) {
                voxelOrigin.y = y;
                for (float z = min.z; z <= max.z; z += scale) {
                    voxelOrigin.z = z;
                    voxelsToCheck.Add(voxelOrigin);

                    ++generatedCount;
                    if (NeedsYield(ref lastYield)) {
                        GeneratedCountText.text = generatedCount.ToString();
                        GeneratedPercentageText.text = string.Format("{0:F2} %", Mathf.Min(100, (generatedCount / totalCount) * 100));
                        TimeTakenText.text = string.Format("{0:F2}s", timer.ElapsedMilliseconds / 1000.0f);
                        yield return null;
                    }
                }
            }
        }
        voxelsToCheck.CompleteAdding();

        GeneratedCountText.text = generatedCount.ToString();
        GeneratedPercentageText.text = string.Format("{0:F2} %", Mathf.Min(100, (generatedCount / totalCount) * 100));

        Debug.Log("Generating Voxels...");
        int loopCount = 0;
        float destroyTotal = totalCount;
        while (true) {
            if (voxelsToGenerate.TryTake(out voxelOrigin)) {
                destroyTotal = Mathf.Max(destroyTotal, voxelsToGenerate.Count);
                voxelMap[voxelOrigin] = 1;

                ++voxelCount;
                DestroyedCountText.text = voxelCount.ToString();
                DestroyedPercentageText.text = string.Format("{0:F2} %", Mathf.Min(100, (voxelCount / destroyTotal) * 100));
            } else if (consumers.All(consumer => consumer.IsCompleted)) {
                break;
            }

            loopCount++;
            if (NeedsYield(ref lastYield)) {
                TimeTakenText.text = string.Format("{0:F2}s", timer.ElapsedMilliseconds / 1000.0f);
                yield return null;
            }
        }

        Debug.Log("Serializing Map...");
        var task = Task<string>.Factory.StartNew(() => {
            return voxelMap.ToJson();
        });

        while (!task.IsCompleted) {
            TimeTakenText.text = string.Format("{0:F2}s", timer.ElapsedMilliseconds / 1000.0f);
            yield return null;
        }

        Debug.Log("Saving...");

        string uniquePath = AssetDatabase.GenerateUniqueAssetPath("map.json");
        string fullPath = Application.dataPath + "/" + uniquePath;
        File.WriteAllText(fullPath, task.Result);

        TimeTakenText.text = string.Format("{0:F2}s", timer.ElapsedMilliseconds / 1000.0f);
        Debug.Log("Finished...");
    }

    private bool NeedsYield(ref Stopwatch lastYeild) {
        if (lastYeild.ElapsedMilliseconds > 100) {
            lastYeild.Restart();
            return true;
        } else {
            return false;
        }
    }

    private void CheckVoxels(Bounds bounds, int[] triangles, Vector3[] vertices) {
        while (true) {
            Vector3 voxelOrigin;
            if (voxelsToCheck.TryTake(out voxelOrigin, 100)) {
                if (MeshExtensions.Contains(bounds, triangles, vertices, voxelOrigin)) {
                    voxelsToGenerate.Add(voxelOrigin);
                }
            } else if (voxelsToCheck.IsAddingCompleted) {
                return;
            }
        }
    }
}
