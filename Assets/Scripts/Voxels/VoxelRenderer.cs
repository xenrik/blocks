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

    private float halfScale;

    // Use this for initialization
    void Start() {
        voxelMap = new VoxelMap();
        voxelMap.FromJson(VoxelMapData.text);

        halfScale = voxelMap.Scale; // / 2.0f;
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

        Dictionary<int, VoxelMesh> meshes = new Dictionary<int, VoxelMesh>();
        foreach (var voxel in voxelMap) {
            ProgressMonitor.Worked(1);
            if (NeedsYield(yieldTimer)) {
                yield return null;
            }

            if (voxel.Value == 0) {
                continue;
            }

            VoxelMesh voxelMesh;
            if (!meshes.TryGetValue(voxel.Value, out voxelMesh)) {
                voxelMesh = new VoxelMesh();

                VoxelDefinition voxelDef = VoxelRegistry.GetRegistry()[voxel.Value];
                GameObject voxelPrefab = voxelDef.VoxelPrefab;

                voxelMesh.Material = voxelPrefab.GetComponent<MeshRenderer>().sharedMaterial;
                voxelMesh.PrefabUvs = voxelPrefab.GetComponent<MeshFilter>().sharedMesh.uv;

                meshes[voxel.Value] = voxelMesh;
            }

            foreach (FaceDirection dir in directions) {
                if (GetAdjacentVoxel(voxelMap, voxel.Key, dir) == 0) {
                    // Render face
                    Face face = new Face(voxel.Key, dir, halfScale);
                    voxelMesh.Add(face);

                    if (voxelMesh.Vertices.Count > 65500) {
                        BuildMesh(voxelMesh);
                        meshes.Remove(voxel.Value);
                    }
                } // else don't need a face here
            }
        }

        foreach (VoxelMesh voxelMesh in meshes.Values) {
            BuildMesh(voxelMesh);
        }

        // We're done!
        ProgressMonitor.Finished();

        Debug.Log($"Completed in {timer.ElapsedMilliseconds / 1000.0f:F2}s");
    }

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
            return voxelMap[adjacentVoxel];
        }
    }

    private void BuildMesh(VoxelMesh voxelMesh) {
        Mesh mesh = new Mesh();

        mesh.SetVertices(voxelMesh.GetVertices());
        mesh.triangles = voxelMesh.Triangles.ToArray();
        mesh.colors = voxelMesh.Colours.ToArray();
        mesh.uv = voxelMesh.Uvs.ToArray();
        mesh.RecalculateBounds();

        GameObject meshGO = new GameObject();
        meshGO.transform.parent = gameObject.transform;
        meshGO.transform.localPosition = Vector3.zero;
        meshGO.transform.localRotation = Quaternion.identity;

        MeshFilter filter = meshGO.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer renderer = meshGO.AddComponent<MeshRenderer>();
        renderer.material = voxelMesh.Material;

        meshGO.AddComponent<MeshCollider>();
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
        private static readonly float THIRD   = 1.0f / 3.0f;
        private static readonly float THIRD_2 = 2.0f / 3.0f;

        private static readonly Vector2 UV_00 = new Vector2(0, 0);
        private static readonly Vector2 UV_01 = new Vector2(0, THIRD);
        private static readonly Vector2 UV_02 = new Vector2(0, THIRD_2);
        private static readonly Vector2 UV_03 = new Vector2(0, 1);
        private static readonly Vector2 UV_10 = new Vector2(THIRD, 0);
        private static readonly Vector2 UV_11 = new Vector2(THIRD, THIRD);
        private static readonly Vector2 UV_12 = new Vector2(THIRD, THIRD_2);
        private static readonly Vector2 UV_13 = new Vector2(THIRD, 1);
        private static readonly Vector2 UV_20 = new Vector2(THIRD_2, 0);
        private static readonly Vector2 UV_21 = new Vector2(THIRD_2, THIRD);
        private static readonly Vector2 UV_22 = new Vector2(THIRD_2, THIRD_2);
        private static readonly Vector2 UV_23 = new Vector2(THIRD_2, 1);
        private static readonly Vector2 UV_30 = new Vector2(1, 0);
        private static readonly Vector2 UV_31 = new Vector2(1, THIRD);
        private static readonly Vector2 UV_32 = new Vector2(1, THIRD_2);
        private static readonly Vector2 UV_33 = new Vector2(1, 1);

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

        public Vector2 uva {
            get {
                switch (dir) {
                case FaceDirection.TOP: return UV_22;
                case FaceDirection.BOTTOM: return UV_10;

                case FaceDirection.LEFT: return UV_20;
                case FaceDirection.RIGHT: return UV_31;

                case FaceDirection.FRONT: return UV_11;
                case FaceDirection.BACK: return UV_13;

                default: return Vector2.zero;
                }
            }
        }

        public Vector2 uvb {
            get {
                switch (dir) {
                case FaceDirection.TOP: return UV_21;
                case FaceDirection.BOTTOM: return UV_00;

                case FaceDirection.LEFT: return UV_10;
                case FaceDirection.RIGHT: return UV_30;

                case FaceDirection.FRONT: return UV_01;
                case FaceDirection.BACK: return UV_12;

                default: return Vector2.zero;
                }
            }
        }

        public Vector2 uvc {
            get {
                switch (dir) {
                case FaceDirection.TOP: return UV_12;
                case FaceDirection.BOTTOM: return UV_11;

                case FaceDirection.LEFT: return UV_21;
                case FaceDirection.RIGHT: return UV_21;

                case FaceDirection.FRONT: return UV_12;
                case FaceDirection.BACK: return UV_03;

                default: return Vector2.zero;
                }
            }
        }

        public Vector2 uvd {
            get {
                switch (dir) {
                case FaceDirection.TOP: return UV_11;
                case FaceDirection.BOTTOM: return UV_01;

                case FaceDirection.LEFT: return UV_11;
                case FaceDirection.RIGHT: return UV_20;

                case FaceDirection.FRONT: return UV_02;
                case FaceDirection.BACK: return UV_02;

                default: return Vector2.zero;
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

    private class VoxelMeshWithDictionary {
        public Dictionary<Vector3, int> Vertices = new Dictionary<Vector3, int>();
        public List<Vector2> Uvs = new List<Vector2>();
        public Vector2[] PrefabUvs;
        public List<int> Triangles = new List<int>();
        public List<Color> Colours = new List<Color>();
        public Material Material;

        public void Add(Face face) {
            int posA = Vertices.AddIfAbsent(face.a, Vertices.Count);
            int posB = Vertices.AddIfAbsent(face.b, Vertices.Count);
            int posC = Vertices.AddIfAbsent(face.c, Vertices.Count);
            int posD = Vertices.AddIfAbsent(face.d, Vertices.Count);

            if (posA >= Uvs.Count) { Uvs.Add(new Vector2(0, 0)); }
            if (posB >= Uvs.Count) { Uvs.Add(new Vector2(1, 0)); }
            if (posC >= Uvs.Count) { Uvs.Add(new Vector2(0, 1)); }
            if (posD >= Uvs.Count) { Uvs.Add(new Vector2(1, 1)); }

            if (posA >= Colours.Count) { Colours.Add(face.color); }
            if (posB >= Colours.Count) { Colours.Add(face.color); }
            if (posC >= Colours.Count) { Colours.Add(face.color); }
            if (posD >= Colours.Count) { Colours.Add(face.color); }

            Triangles.AddRange(new int[] { posA, posB, posC });
            Triangles.AddRange(new int[] { posB, posD, posC });
        }

        public List<Vector3> GetVertices() {
            var vertexList = Vertices.ToList();
            vertexList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            return vertexList.Select(pair => pair.Key).ToList();
        }
    }

    private class VoxelMeshWithList {
        public List<Vector3> Vertices = new List<Vector3>();
        public List<Vector2> Uvs = new List<Vector2>();
        public Vector2[] PrefabUvs;
        public List<int> Triangles = new List<int>();
        public List<Color> Colours = new List<Color>();
        public Material Material;

        public void Add(Face face) {
            int posA = Vertices.Count; Vertices.Add(face.a);
            int posB = Vertices.Count; Vertices.Add(face.b);
            int posC = Vertices.Count; Vertices.Add(face.c);
            int posD = Vertices.Count; Vertices.Add(face.d);

            Uvs.Add(face.uva);
            Uvs.Add(face.uvb);
            Uvs.Add(face.uvc);
            Uvs.Add(face.uvd);

            Colours.Add(face.color);
            Colours.Add(face.color);
            Colours.Add(face.color);
            Colours.Add(face.color);

            Triangles.AddRange(new int[] { posA, posB, posC });
            Triangles.AddRange(new int[] { posB, posD, posC });
        }

        public List<Vector3> GetVertices() {
            return Vertices;
        }
    }

    private class VoxelMesh : VoxelMeshWithList {
    }
}
