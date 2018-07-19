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
    public float VoxelScale;

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

        halfScale = VoxelScale / 2.0f;

        voxelMap = new VoxelMap();
        voxelMap.FromJson(VoxelMapData.text);
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

        // Work out which faces we need. The int represents the direction of face:
        // 1 - Clockwise face
        // 2 - Counter Clockwise face
        // 3 - Both (i.e. not needed)

        ProgressMonitor.Begin("Building Mesh");
        ProgressMonitor.BeginSubtask("Generating Faces", voxelMap.Count);
        var directions = System.Enum.GetValues(typeof(FaceDirection));

        /*
        ConcurrentDictionary<Vector3,Face> faces = new ConcurrentDictionary<Vector3,Face>();
        var options = new ParallelOptions();
        options.MaxDegreeOfParallelism = 4;
        var task = Task.Factory.StartNew(() => {
            Parallel.ForEach(voxelMap, options,
                (entry) => {
                    ProgressMonitor.Worked(1);
                    foreach (FaceDirection face in directions) {
                        PopulateFace(faces, entry.Key, face);
                    }
                });
        });
           
        while (!task.IsCompleted) {
            yield return null;
        } 
        */

        int[] min = { int.MaxValue, int.MaxValue, int.MaxValue };
        int[] max = { int.MinValue, int.MinValue,  int.MinValue };
        foreach (var entry in voxelMap) {
            if (NeedsYield(yieldTimer)) {
                yield return null;
            }

            IntVector3 origin = entry.Key;

            foreach (FaceDirection face in directions) {
                switch (face) {
                case FaceDirection.TOP: origin.y--; break;
                case FaceDirection.BOTTOM: origin.y++; break;
                case FaceDirection.LEFT: origin.x--; break;
                case FaceDirection.RIGHT: origin.x++; break;
                case FaceDirection.BACK: origin.z--; break;
                case FaceDirection.FRONT: origin.z++; break;
                }

                min[0] = Mathf.Min(min[0], origin.x);
                max[0] = Mathf.Max(max[0], origin.x);

                min[1] = Mathf.Min(min[1], origin.y);
                max[1] = Mathf.Max(max[1], origin.y);

                min[2] = Mathf.Min(min[2], origin.z);
                max[2] = Mathf.Max(max[2], origin.z);
            }
        }


        Debug.Log($"Need array of size: {(max[0] - min[0]) * (max[1] - min[1]) * (max[2] - min[2])}");

        bool[,,] faces = new bool[max[0] - min[0] + 1,max[1] - min[1] + 1,max[2] - min[2] + 1];
        foreach (var entry in voxelMap) {
            ProgressMonitor.Worked(1);
            if (NeedsYield(yieldTimer)) {
                yield return null;
            }

            IntVector3 origin = entry.Key;

            foreach (FaceDirection face in directions) {
                switch (face) {
                case FaceDirection.TOP: origin.y--; break;
                case FaceDirection.BOTTOM: origin.y++; break;
                case FaceDirection.LEFT: origin.x--; break;
                case FaceDirection.RIGHT: origin.x++; break;
                case FaceDirection.BACK: origin.z--; break;
                case FaceDirection.FRONT: origin.z++; break;
                }

                int x = origin.x - min[0];
                int y = origin.y - min[1];
                int z = origin.z - min[2];

                if (x < 0 || y < 0 || z < 0 ||
                    x >= faces.GetLength(0) ||
                    y >= faces.GetLength(1) ||
                    z >= faces.GetLength(2)) {
                    Debug.Log($"{x}-{y}-{z} vs {faces.GetLength(0)}-{faces.GetLength(1)}-{faces.GetLength(2)}");
                }
                faces[x, y, z] = !faces[x, y, z];
            }
        }

        ProgressMonitor.BeginSubtask("Generating Faces", voxelMap.Count);
        var vertices = new Dictionary<Vector3, int>();
        var colors = new List<Color>();
        var triangles = new List<int>();

        foreach (var entry in voxelMap) {
            ProgressMonitor.Worked(1);
            if (NeedsYield(yieldTimer)) {
                yield return null;
            }

            IntVector3 origin = entry.Key;

            foreach (FaceDirection dir in directions) {
                switch (dir) {
                case FaceDirection.TOP: origin.y--; break;
                case FaceDirection.BOTTOM: origin.y++; break;
                case FaceDirection.LEFT: origin.x--; break;
                case FaceDirection.RIGHT: origin.x++; break;
                case FaceDirection.BACK: origin.z--; break;
                case FaceDirection.FRONT: origin.z++; break;
                }

                int x = origin.x - min[0];
                int y = origin.y - min[1];
                int z = origin.z - min[2];

                if (faces[x, y, z]) {
                    Face face = new Face(origin, dir, halfScale);

                    int a = vertices.AddIfAbsent(face.a, vertices.Count);
                    int b = vertices.AddIfAbsent(face.b, vertices.Count);
                    int c = vertices.AddIfAbsent(face.c, vertices.Count);
                    int d = vertices.AddIfAbsent(face.d, vertices.Count);

                    if (colors.Count <= a) { colors.Add(face.color); }
                    if (colors.Count <= b) { colors.Add(face.color); }
                    if (colors.Count <= c) { colors.Add(face.color); }
                    if (colors.Count <= d) { colors.Add(face.color); }

                    triangles.Add(a); triangles.Add(b); triangles.Add(c);
                    triangles.Add(b); triangles.Add(d); triangles.Add(c);

                    if (vertices.Count > 65534) {
                        Debug.Log("Vertex limit hit!");
                        BuildMesh(vertices, triangles, colors);

                        vertices.Clear();
                        triangles.Clear();
                        colors.Clear();
                    }
                }
            }
        }

        if (vertices.Count > 0) {
            BuildMesh(vertices, triangles, colors);
        }

        /*
        Dictionary<Vector3, Face> faces = new Dictionary<Vector3, Face>(voxelMap.Count * 6);
        foreach (var entry in voxelMap) {
            ProgressMonitor.Worked(1);
            if (NeedsYield(yieldTimer)) {
                yield return null;
            }

            foreach (FaceDirection face in directions) {
                PopulateFace(faces, entry.Key, face);
            }
        };

        // Build the mesh including and faces that have only clockwise or counter-clockwise
        // directions (not both)
        var vertices = new Dictionary<Vector3, int>();
        //var vertices = new List<Vector3>();
        var colors = new List<Color>();
        var triangles = new List<int>();

        ProgressMonitor.BeginSubtask("Generating Mesh", faces.Count);
        int faceCount = 0;
        foreach (var face in faces.Values) {
            ProgressMonitor.Worked(1);
            if (NeedsYield(yieldTimer)) {
                yield return null;
            }

            if (face.count != 1) {
                continue;
            }

            ++faceCount;

            int a = vertices.AddIfAbsent(face.a, vertices.Count);
            int b = vertices.AddIfAbsent(face.b, vertices.Count);
            int c = vertices.AddIfAbsent(face.c, vertices.Count);
            int d = vertices.AddIfAbsent(face.d, vertices.Count);

            if (colors.Count <= a) { colors.Add(face.color); }
            if (colors.Count <= b) { colors.Add(face.color); }
            if (colors.Count <= c) { colors.Add(face.color); }
            if (colors.Count <= d) { colors.Add(face.color); }

            triangles.Add(a); triangles.Add(b); triangles.Add(c);
            triangles.Add(b); triangles.Add(d); triangles.Add(c);

            if (vertices.Count > 65534) {
                Debug.Log("Vertex limit hit!");
                BuildMesh(vertices, triangles, colors);

                vertices.Clear();
                triangles.Clear();
                colors.Clear();
            }
        }
        Debug.Log($"Rendered {faceCount} faces");

        if (vertices.Count > 0) {
            BuildMesh(vertices, triangles, colors);
        }
        */

        // We're done!
        meshRenderer.enabled = true;
        ProgressMonitor.Finished();

        Debug.Log($"Completed in {timer.ElapsedMilliseconds / 1000.0f:F2}s");
    }

    private void BuildMesh(IDictionary<Vector3, int> vertices, List<int> triangles, List<Color> colors) {
        Mesh mesh = new Mesh();

        var vertexList = vertices.ToList();
        vertexList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
        mesh.SetVertices(vertexList.Select(pair => pair.Key).ToList());
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
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

    int logged = 0;

    private void PopulateFace(IDictionary<Vector3, Face> faces, Vector3 origin, FaceDirection dir) {
        switch (dir) {
        case FaceDirection.TOP: origin.y -= halfScale; break;
        case FaceDirection.BOTTOM: origin.y += halfScale; break;
        case FaceDirection.LEFT: origin.x -= halfScale; break;
        case FaceDirection.RIGHT: origin.x += halfScale; break;
        case FaceDirection.BACK: origin.z -= halfScale; break;
        case FaceDirection.FRONT: origin.z += halfScale; break;
        }

        /*
        Face face = faces.GetOrDefault(origin);
        face.count++;
        if (face.count == 1) {
            faces[origin] = new Face(origin, dir, halfScale);
        }
        */

        if (logged < 24) {
            Debug.Log(origin);
            ++logged;
        }

        Face face = null;
        if (faces.TryGetValue(origin, out face)) {
            face.count++;
        } else {
            //faces[origin] = new Face(origin, dir, halfScale);
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
                case FaceDirection.TOP: return new Vector3(p[2], p[0], p[3]);
                case FaceDirection.BOTTOM: return new Vector3(p[2], p[0], p[3]);

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
                case FaceDirection.TOP: return new Vector3(p[1], p[0], p[4]);
                case FaceDirection.BOTTOM: return new Vector3(p[1], p[0], p[4]);

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
                case FaceDirection.TOP: return new Vector3(p[2], p[0], p[2]);
                case FaceDirection.BOTTOM: return new Vector3(p[1], p[0], p[3]);

                case FaceDirection.LEFT: return new Vector3(p[0], p[1], p[3]);
                case FaceDirection.RIGHT: return new Vector3(p[0], p[2], p[4]);

                case FaceDirection.FRONT: return new Vector3(p[2], p[4], p[0]);
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

            switch (dir) {
            case FaceDirection.TOP: // Correct
            case FaceDirection.BOTTOM: // Correct
                p[0] = origin.y;
                p[1] = origin.x + halfScale;
                p[2] = origin.x - halfScale;
                p[3] = origin.z + halfScale;
                p[4] = origin.z - halfScale;
                break;

            case FaceDirection.LEFT: // Correct
            case FaceDirection.RIGHT:
                p[0] = origin.x;
                p[1] = origin.y + halfScale;
                p[2] = origin.y - halfScale;
                p[3] = origin.z + halfScale;
                p[4] = origin.z - halfScale;
                break;

            case FaceDirection.FRONT:
            case FaceDirection.BACK: // Correct
                p[0] = origin.z;
                p[1] = origin.x + halfScale;
                p[2] = origin.x - halfScale;
                p[3] = origin.y + halfScale;
                p[4] = origin.y - halfScale;
                break;
            }

            /*
            switch (dir) {
            case FaceDirection.TOP: // Correct
                a.x += halfScale; a.z += halfScale;
                b.x -= halfScale; b.z += halfScale;
                c.x += halfScale; c.z -= halfScale;
                d.x -= halfScale; d.z -= halfScale;
                color = Color.red;
                break;

            case FaceDirection.BOTTOM: // Correct
                a.x -= halfScale; a.z -= halfScale;
                b.x -= halfScale; b.z += halfScale;
                c.x += halfScale; c.z -= halfScale;
                d.x += halfScale; d.z += halfScale;
                color = Color.magenta;
                break;

            case FaceDirection.LEFT: // Correct
                a.y -= halfScale; a.z -= halfScale;
                b.y -= halfScale; b.z += halfScale;
                c.y += halfScale; c.z -= halfScale;
                d.y += halfScale; d.z += halfScale;
                color = Color.green;

                break;

            case FaceDirection.RIGHT:
                a.y += halfScale; a.z += halfScale;
                b.y -= halfScale; b.z += halfScale;
                c.y += halfScale; c.z -= halfScale;
                d.y -= halfScale; d.z -= halfScale;
                color = Color.yellow;

                break;

            case FaceDirection.FRONT:
                a.x -= halfScale; a.y -= halfScale;
                b.x += halfScale; b.y -= halfScale;
                c.x -= halfScale; c.y += halfScale;
                d.x += halfScale; d.y += halfScale;
                color = Color.blue;

                break;

            case FaceDirection.BACK: // Correct
                a.x += halfScale; a.y += halfScale;
                b.x += halfScale; b.y -= halfScale;
                c.x -= halfScale; c.y += halfScale;
                d.x -= halfScale; d.y -= halfScale;
                color = Color.cyan;

                break;
            }*/
        }
    }

    private enum FaceDirection {
        TOP, BOTTOM,
        LEFT, RIGHT,
        BACK, FRONT
    }
}
