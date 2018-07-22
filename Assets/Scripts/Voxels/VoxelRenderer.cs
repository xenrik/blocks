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

    public UIProgressMonitor ProgressMonitor;

    private VoxelMap voxelMap;
    private int mapHashCode;

    private GameObject meshGameObject;
    private MeshRenderer meshRenderer;

    private float halfScale;

    // Use this for initialization
    void Start() {
        //meshGameObject = new GameObject("VoxelMesh");
        //meshFilter = meshGameObject.AddComponent<MeshFilter>();
        //meshRenderer = meshGameObject.AddComponent<MeshRenderer>();

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;

        voxelMap = new VoxelMap();
        voxelMap.FromJson(VoxelMapData.text);

        halfScale = voxelMap.Scale / 2.0f;
    }

    // Update is called once per frame
    void Update() {
        if (mapHashCode != voxelMap.GetHashCode()) {
            mapHashCode = voxelMap.GetHashCode();

            StartCoroutine(BuildMesh());
        }
    }

    private IEnumerator BuildMesh() {
        Stopwatch timer = new Stopwatch();
        timer.Start();

        Stopwatch yieldTimer = new Stopwatch();
        yieldTimer.Start();

        Debug.Log("Building Mesh...");

        // Work our which faces we need. We do this by checking to see if a two voxels share and adjacent
        // side.

        var directions = System.Enum.GetValues(typeof(FaceDirection));
        ProgressMonitor.Begin("Building Mesh", voxelMap.Size);

        Dictionary<Vector3, int> vertices = new Dictionary<Vector3, int>();
        //List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colours = new List<Color>();
        bool doBreak = false;
        foreach (var voxel in voxelMap) {
            ProgressMonitor.Worked(1);
            if (NeedsYield(yieldTimer)) {
                yield return null;
            }

            if (voxel.Value == 0) {
                continue;
            }

            foreach (FaceDirection dir in directions) {
                if (GetAdjacentVoxel(voxelMap, voxel.Key, dir) == 0) {
                    // Render face
                    Face face = new Face(voxel.Key, dir, halfScale);

                    int a = vertices.AddIfAbsent(face.a, vertices.Count);
                    int b = vertices.AddIfAbsent(face.b, vertices.Count);
                    int c = vertices.AddIfAbsent(face.c, vertices.Count);
                    int d = vertices.AddIfAbsent(face.d, vertices.Count);

                    //int a = vertices.Count; vertices.Add(face.a);
                    //int b = vertices.Count; vertices.Add(face.b);
                    //int c = vertices.Count; vertices.Add(face.c);
                    //int d = vertices.Count; vertices.Add(face.d);

                    if (colours.Count <= a) { colours.Add(face.color); }
                    if (colours.Count <= b) { colours.Add(face.color); }
                    if (colours.Count <= c) { colours.Add(face.color); }
                    if (colours.Count <= d) { colours.Add(face.color); }

                    triangles.Add(a); triangles.Add(b); triangles.Add(c);
                    triangles.Add(b); triangles.Add(d); triangles.Add(c);

                    if (vertices.Count > 65534) {
                        Debug.Log("Vertex limit hit!");

                        var vertexList = vertices.ToList();
                        vertexList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
                        BuildMesh(vertexList.Select(pair => pair.Key).ToList(), triangles, colours);
                        //BuildMesh(vertices, triangles, colours);

                        vertices.Clear();
                        triangles.Clear();
                        colours.Clear();

                        //doBreak = true;
                        //break;
                    }
                } // else don't need a face here
            }

            if (doBreak) {
                break;
            }
        }

        if (vertices.Count > 0) {
            var vertexList = vertices.ToList();
            vertexList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            BuildMesh(vertexList.Select(pair => pair.Key).ToList(), triangles, colours);
            //BuildMesh(vertices, triangles, colours);
        }

        // We're done!
        meshRenderer.enabled = true;
        ProgressMonitor.Finished();

        Debug.Log($"Completed in {timer.ElapsedMilliseconds / 1000.0f:F2}s");
    }

    int count = 0;

    private int GetAdjacentVoxel(VoxelMap map, IntVector3 key, FaceDirection dir) {
        IntVector3 adjacentVoxel;
        switch (dir) {
        case FaceDirection.TOP: adjacentVoxel = key.Translate(0, 1, 0); break;
        case FaceDirection.BOTTOM: adjacentVoxel = key.Translate(0, -1, 0); break;
        case FaceDirection.RIGHT: adjacentVoxel = key.Translate(1, 0, 0); break;
        case FaceDirection.LEFT: adjacentVoxel = key.Translate(-1, 0, 0); break;
        case FaceDirection.FRONT: adjacentVoxel = key.Translate(0, 0, 1); break;
        case FaceDirection.BACK: adjacentVoxel = key.Translate(0, 0, -1); break;

        default:
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
            if (count < 20) {
                ++count;
                Debug.Log($"{key} - {adjacentVoxel} - {dir} = {voxelMap[adjacentVoxel]}");
            }
            return voxelMap[adjacentVoxel];
        }
    }

    private void BuildMesh(List<Vector3> vertices, List<int> triangles, List<Color> colours) {
        Mesh mesh = new Mesh();

        mesh.SetVertices(vertices);
        mesh.triangles = triangles.ToArray();
        mesh.colors = colours.ToArray();
        mesh.RecalculateBounds();

        GameObject meshGO = new GameObject();
        meshGO.transform.parent = gameObject.transform;
        meshGO.transform.localPosition = Vector3.zero;
        meshGO.transform.localRotation = Quaternion.identity;

        MeshFilter filter = meshGO.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer renderer = meshGO.AddComponent<MeshRenderer>();
        renderer.materials = meshRenderer.materials;
    }
    
    private bool NeedsYield(Stopwatch lastYeild) {
        if (lastYeild.ElapsedMilliseconds > 100) {
            lastYeild.Restart();
            return true;
        } else {
            return false;
        }
    }

    private class Face {
        public int count;

        private float[] p = new float[5];
        private FaceDirection dir;

        public Vector3 a {
            get {
                switch (dir) {
                case FaceDirection.TOP: return new Vector3(p[1], p[0], p[3]);
                case FaceDirection.BOTTOM: return new Vector3(p[2], p[0], p[4]);

                case FaceDirection.LEFT: return new Vector3(p[0], p[2], p[4]);
                case FaceDirection.RIGHT: return new Vector3(p[0], p[1], p[3]);

                case FaceDirection.FRONT: return new Vector3(p[2], p[4], p[0]);
                case FaceDirection.BACK: return new Vector3(p[1], p[3], p[0]);

                default: return Vector3.zero;
                }
            }
        }

        public Vector3 b {
            get {
                switch (dir) {
                case FaceDirection.TOP: return new Vector3(p[1], p[0], p[4]);
                case FaceDirection.BOTTOM: return new Vector3(p[1], p[0], p[4]);

                case FaceDirection.LEFT: return new Vector3(p[0], p[2], p[3]);
                case FaceDirection.RIGHT: return new Vector3(p[0], p[2], p[3]);

                case FaceDirection.FRONT: return new Vector3(p[1], p[4], p[0]);
                case FaceDirection.BACK: return new Vector3(p[1], p[4], p[0]);

                default: return Vector3.zero;
                }
            }
        }

        public Vector3 c {
            get {
                switch (dir) {
                case FaceDirection.TOP: return new Vector3(p[2], p[0], p[3]);
                case FaceDirection.BOTTOM: return new Vector3(p[2], p[0], p[3]);

                case FaceDirection.LEFT: return new Vector3(p[0], p[1], p[4]);
                case FaceDirection.RIGHT: return new Vector3(p[0], p[1], p[4]);

                case FaceDirection.FRONT: return new Vector3(p[2], p[3], p[0]);
                case FaceDirection.BACK: return new Vector3(p[2], p[3], p[0]);

                default: return Vector3.zero;
                }
            }
        }

        public Vector3 d {
            get {
                switch (dir) {
                case FaceDirection.TOP: return new Vector3(p[2], p[0], p[4]);
                case FaceDirection.BOTTOM: return new Vector3(p[1], p[0], p[3]);

                case FaceDirection.LEFT: return new Vector3(p[0], p[1], p[3]);
                case FaceDirection.RIGHT: return new Vector3(p[0], p[2], p[4]);

                case FaceDirection.FRONT: return new Vector3(p[1], p[3], p[0]);
                case FaceDirection.BACK: return new Vector3(p[2], p[4], p[0]);

                default: return Vector3.zero;
                }
            }
        }

        public Color color {
            get {
                switch (dir) {
                case FaceDirection.TOP: return Color.red;
                case FaceDirection.BOTTOM: return Color.magenta;

                case FaceDirection.LEFT: return Color.green;
                case FaceDirection.RIGHT: return Color.yellow;

                case FaceDirection.FRONT: return Color.blue;
                case FaceDirection.BACK: return Color.cyan;

                default: return Color.clear;
                }
            }
        }

        public Face(Vector3 origin, FaceDirection dir, float halfScale) {
            count = 1;
            this.dir = dir;
            origin = origin * halfScale * 2;

            switch (dir) {
            case FaceDirection.TOP:
                p[0] = origin.y + halfScale;
                p[1] = origin.x + halfScale;
                p[2] = origin.x - halfScale;
                p[3] = origin.z + halfScale;
                p[4] = origin.z - halfScale;
                break;

            case FaceDirection.BOTTOM:
                p[0] = origin.y - halfScale;
                p[1] = origin.x + halfScale;
                p[2] = origin.x - halfScale;
                p[3] = origin.z + halfScale;
                p[4] = origin.z - halfScale;
                break;

            case FaceDirection.LEFT:
                p[0] = origin.x - halfScale;
                p[1] = origin.y + halfScale;
                p[2] = origin.y - halfScale;
                p[3] = origin.z + halfScale;
                p[4] = origin.z - halfScale;
                break;

            case FaceDirection.RIGHT:
                p[0] = origin.x + halfScale;
                p[1] = origin.y + halfScale;
                p[2] = origin.y - halfScale;
                p[3] = origin.z + halfScale;
                p[4] = origin.z - halfScale;
                break;

            case FaceDirection.FRONT:
                p[0] = origin.z + halfScale;
                p[1] = origin.x + halfScale;
                p[2] = origin.x - halfScale;
                p[3] = origin.y + halfScale;
                p[4] = origin.y - halfScale;
                break;

            case FaceDirection.BACK: 
                p[0] = origin.z - halfScale;
                p[1] = origin.x + halfScale;
                p[2] = origin.x - halfScale;
                p[3] = origin.y + halfScale;
                p[4] = origin.y - halfScale;
                break;
            }
        }
    }

    private enum FaceDirection {
        TOP, BOTTOM,
        LEFT, RIGHT,
        BACK, FRONT
    }
}
