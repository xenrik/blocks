using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelRenderer : MonoBehaviour {

    /** The map we are rendering */
    public VoxelMap VoxelMap;

    private int mapHashCode;

    private Mesh mesh;

    private GameObject meshGameObject;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

	// Use this for initialization
	void Start () {
        meshGameObject = new GameObject("VoxelMesh");
        meshFilter = meshGameObject.AddComponent<MeshFilter>();
        meshRenderer = meshGameObject.AddComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (VoxelMap == null) {
            return;
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
            foreach (FaceDirection face in System.Enum.GetValues(typeof(Face))) {
                PopulateFace(faces, entry.Key, face);
            }
        }

        // Build the mesh including and faces that have only clockwise or counter-clockwise
        // directions (not both)
        var vertices = new Dictionary<Vector3, int>();
        var triangles = new List<int>();

        foreach (var face in faces.Values) {
            if (face.ignored) {
                continue;
            }

            int a = vertices.AddIfAbsent(face.a, vertices.Count);
            int b = vertices.AddIfAbsent(face.b, vertices.Count);
            int c = vertices.AddIfAbsent(face.c, vertices.Count);
            int d = vertices.AddIfAbsent(face.d, vertices.Count);

            triangles.AddRange(new int[] { a, b, c });
            triangles.AddRange(new int[] { b, c, d });
        }

        Mesh mesh = new Mesh();

        // Sort the vertices and add to the mesh
        var vertexList = vertices.ToList();
        vertexList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
        mesh.SetVertices(vertexList.Select(pair => pair.Key).ToList());

        // Assign the triangles
        mesh.triangles = triangles.ToArray();

        // Set onto the filter and we're done!
        meshFilter.sharedMesh = mesh;

        Debug.Log("Done!");
    }

    private static readonly float FACE_SIZE = 0.5f;

    private void PopulateFace(Dictionary<Vector3, Face> faces, Vector3 origin, FaceDirection dir) {
        switch (dir) {
        case FaceDirection.TOP: origin.y -= FACE_SIZE; break;
        case FaceDirection.BOTTOM: origin.y += FACE_SIZE; break;
        case FaceDirection.LEFT: origin.x -= FACE_SIZE; break;
        case FaceDirection.RIGHT: origin.x += FACE_SIZE; break;
        case FaceDirection.BACK: origin.x -= FACE_SIZE; break;
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

        public Face(Vector3 origin, FaceDirection dir) {
            a = origin;
            b = origin;
            c = origin;
            d = origin;

            switch (dir) {
            case FaceDirection.TOP:
                a.x -= FACE_SIZE; a.z -= FACE_SIZE;
                b.x -= FACE_SIZE; b.z += FACE_SIZE;
                c.x += FACE_SIZE; c.z -= FACE_SIZE;
                d.x += FACE_SIZE; d.z += FACE_SIZE;
                break;

            case FaceDirection.BOTTOM:
                a.x += FACE_SIZE; a.z += FACE_SIZE;
                b.x += FACE_SIZE; b.z -= FACE_SIZE;
                c.x -= FACE_SIZE; c.z += FACE_SIZE;
                d.x -= FACE_SIZE; d.z -= FACE_SIZE;
                break;

            case FaceDirection.LEFT:
                a.y -= FACE_SIZE; a.z -= FACE_SIZE;
                b.y -= FACE_SIZE; b.z += FACE_SIZE;
                c.y += FACE_SIZE; c.z -= FACE_SIZE;
                d.y += FACE_SIZE; d.z += FACE_SIZE;
                break;

            case FaceDirection.RIGHT:
                a.y += FACE_SIZE; a.z += FACE_SIZE;
                b.y += FACE_SIZE; b.z -= FACE_SIZE;
                c.y -= FACE_SIZE; c.z += FACE_SIZE;
                d.y -= FACE_SIZE; d.z -= FACE_SIZE;
                break;

            case FaceDirection.FRONT:
                a.x -= FACE_SIZE; a.y -= FACE_SIZE;
                b.x -= FACE_SIZE; b.y += FACE_SIZE;
                c.x += FACE_SIZE; c.y -= FACE_SIZE;
                d.x += FACE_SIZE; d.y += FACE_SIZE;
                break;

            case FaceDirection.BACK:
                a.x += FACE_SIZE; a.y += FACE_SIZE;
                b.x += FACE_SIZE; b.y -= FACE_SIZE;
                c.x -= FACE_SIZE; c.y += FACE_SIZE;
                d.x -= FACE_SIZE; d.y -= FACE_SIZE;
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
