using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelRenderer : MonoBehaviour {

    /** The map we are rendering */
    public VoxelMap VoxelMap;

    private int mapHashCode;

    //private MeshFilter meshFilter;

    private GameObject meshGameObject;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;


    // Use this for initialization
    void Start () {
       //meshGameObject = new GameObject("VoxelMesh");
       //meshFilter = meshGameObject.AddComponent<MeshFilter>();
       //meshRenderer = meshGameObject.AddComponent<MeshRenderer>();

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (VoxelMap == null) {
            Debug.Log("No voxel map");
            return;
        } else if (VoxelMap.Count == 0) {
            Debug.Log("No voxels on map!");
            VoxelMap.AfterDeserialize();
            if (VoxelMap.Count == 0) {
                Debug.Log("Manual deserialize failed to fix it");
                return;
            }
        }

        if (mapHashCode != VoxelMap.GetHashCode()) {
            BuildMesh();
            mapHashCode = VoxelMap.GetHashCode();
        }
	}

    private void BuildMesh() {
        Debug.Log("Building Mesh...");

        // Work out which faces we need. The int represents the direction of face:
        // 1 - Clockwise face
        // 2 - Counter Clockwise face
        // 3 - Both (i.e. not needed)

        Dictionary<Vector3,Face> faces = new Dictionary<Vector3,Face>();
        foreach (var entry in VoxelMap) {
            foreach (FaceDirection face in System.Enum.GetValues(typeof(FaceDirection))) {
                PopulateFace(faces, entry.Key, face);
            }
        }

        // Build the mesh including and faces that have only clockwise or counter-clockwise
        // directions (not both)
        //var vertices = new Dictionary<Vector3, int>();
        var vertices = new List<Vector3>();
        var colors = new List<Color>();
        var triangles = new List<int>();

        foreach (var face in faces.Values) {
            if (face.ignored) {
                continue;
            }

            /*
            int a = vertices.AddIfAbsent(face.a, vertices.Count);
            int b = vertices.AddIfAbsent(face.b, vertices.Count);
            int c = vertices.AddIfAbsent(face.c, vertices.Count);
            int d = vertices.AddIfAbsent(face.d, vertices.Count);

            if (colors.Count <= a) { colors.Add(face.color); }
            if (colors.Count <= b) { colors.Add(face.color); }
            if (colors.Count <= c) { colors.Add(face.color); }
            if (colors.Count <= d) { colors.Add(face.color); }
            */

            int a = vertices.Count; vertices.Add(face.a);
            int b = vertices.Count; vertices.Add(face.b);
            int c = vertices.Count; vertices.Add(face.c);
            int d = vertices.Count; vertices.Add(face.d);

            colors.Add(face.color); colors.Add(face.color); colors.Add(face.color); colors.Add(face.color);

            triangles.Add(a); triangles.Add(b); triangles.Add(c);
            triangles.Add(b); triangles.Add(d); triangles.Add(c);
        }

        Mesh mesh = new Mesh();

        // Sort the vertices and add to the mesh
        //var vertexList = vertices.ToList();
        //vertexList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
        //mesh.SetVertices(vertexList.Select(pair => pair.Key).ToList());
        mesh.SetVertices(vertices);

        // Assign the triangles
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();

        // Set onto the filter and we're done!
        meshFilter.mesh = mesh;
        meshRenderer.enabled = true;

        Debug.Log("Done!");
    }

    private static readonly float FACE_SIZE = 0.5f;

    private void PopulateFace(Dictionary<Vector3, Face> faces, Vector3 origin, FaceDirection dir) {
        switch (dir) {
        case FaceDirection.TOP: origin.y -= FACE_SIZE; break;
        case FaceDirection.BOTTOM: origin.y += FACE_SIZE; break;
        case FaceDirection.LEFT: origin.x -= FACE_SIZE; break;
        case FaceDirection.RIGHT: origin.x += FACE_SIZE; break;
        case FaceDirection.BACK: origin.z -= FACE_SIZE; break;
        case FaceDirection.FRONT: origin.z += FACE_SIZE; break;
        }

        Face face = faces.GetOrDefault(origin);
        if (face == null) {
            faces[origin] = new Face(origin, dir);
        } else {
            face.ignored = true;
        }
    }

    private class Face {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
        public Vector3 d;

        public bool ignored;
        public Color color;

        public Face(Vector3 origin, FaceDirection dir) {
            a = origin;
            b = origin;
            c = origin;
            d = origin;

            switch (dir) {
            case FaceDirection.TOP:
                a.x += FACE_SIZE; a.z += FACE_SIZE;
                b.x += FACE_SIZE; b.z -= FACE_SIZE;
                c.x -= FACE_SIZE; c.z += FACE_SIZE;
                d.x -= FACE_SIZE; d.z -= FACE_SIZE;
                color = Color.red;
                break;

            case FaceDirection.BOTTOM:
                a.x -= FACE_SIZE; a.z -= FACE_SIZE;
                b.x -= FACE_SIZE; b.z += FACE_SIZE;
                c.x += FACE_SIZE; c.z -= FACE_SIZE;
                d.x += FACE_SIZE; d.z += FACE_SIZE;
                color = Color.magenta;
                break;

            case FaceDirection.LEFT:
                a.y -= FACE_SIZE; a.z -= FACE_SIZE;
                b.y -= FACE_SIZE; b.z += FACE_SIZE;
                c.y += FACE_SIZE; c.z -= FACE_SIZE;
                d.y += FACE_SIZE; d.z += FACE_SIZE;
                color = Color.green;

                break;

            case FaceDirection.RIGHT:
                a.y += FACE_SIZE; a.z += FACE_SIZE;
                b.y += FACE_SIZE; b.z -= FACE_SIZE;
                c.y -= FACE_SIZE; c.z += FACE_SIZE;
                d.y -= FACE_SIZE; d.z -= FACE_SIZE;
                color = Color.yellow;

                break;

            case FaceDirection.FRONT:
                a.x -= FACE_SIZE; a.y -= FACE_SIZE;
                b.x -= FACE_SIZE; b.y += FACE_SIZE;
                c.x += FACE_SIZE; c.y -= FACE_SIZE;
                d.x += FACE_SIZE; d.y += FACE_SIZE;
                color = Color.blue;

                break;

            case FaceDirection.BACK:
                a.x += FACE_SIZE; a.y += FACE_SIZE;
                b.x += FACE_SIZE; b.y -= FACE_SIZE;
                c.x -= FACE_SIZE; c.y += FACE_SIZE;
                d.x -= FACE_SIZE; d.y -= FACE_SIZE;
                color = Color.cyan;

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
