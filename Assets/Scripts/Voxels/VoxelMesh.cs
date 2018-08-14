using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class VoxelMesh {

    private const int MAX_FACES_PER_MESH = 5000; //65500 / 4;
    private List<MeshDetails> meshDetailsList = new List<MeshDetails>();

    public void Add(VoxelFace face, Material material) {
        foreach (var meshDetails in meshDetailsList) {
            if (meshDetails.material == material &&
                meshDetails.faces.Count < MAX_FACES_PER_MESH) {
                meshDetails.faces.Add(face);
                meshDetails.dirty = true;

                return;
            }
        }

        var newMeshDetails = new MeshDetails(material);
        newMeshDetails.faces.Add(face);
        meshDetailsList.Add(newMeshDetails);
    }

    public void Remove(VoxelFace face, Material material) {
        foreach (var meshDetails in meshDetailsList) {
            if (meshDetails.material == material &&
                meshDetails.faces.Contains(face)) {
                meshDetails.faces.Remove(face);
                meshDetails.dirty = true;

                return;
            }
        }
    }

    public void Update() {
        foreach (var meshDetails in meshDetailsList) {
            meshDetails.Update();
        }
    }

    /**
     * Get the game objects this mesh is using
     */
    public GameObject[] GetGameObjects() {
        return meshDetailsList.Select(meshDetails => meshDetails.gameObject).ToArray();
    }

    internal class MeshDetails {
        internal Material material;
        internal HashSet<VoxelFace> faces = new HashSet<VoxelFace>();
        internal GameObject gameObject;

        internal Mesh mesh;
        internal bool dirty;

        private MeshCollider collider;

        private Vector3[] vertices;
        private Vector2[] uvs;
        private int[] triangles;

        internal MeshDetails(Material material) {
            this.material = material;

            mesh = new Mesh();
            mesh.MarkDynamic();

            gameObject = new GameObject();

            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;

            collider = gameObject.AddComponent<MeshCollider>();
            dirty = true;
        }

        internal void Update() {
            if (!dirty) {
                return;
            }

            if (vertices == null || vertices.Length < faces.Count * 4) {
                vertices = new Vector3[faces.Count * 4];
            }
            if (uvs == null || uvs.Length != vertices.Length) {
                uvs = new Vector2[faces.Count * 4];
            }
            if (triangles == null || triangles.Length < faces.Count * 6) {
                triangles = new int[faces.Count * 6];
            }

            int vertexOffset = 0;
            int uvOffset = 0;
            int triangleOffset = 0;

            foreach (var face in faces) {
                int vertexA = vertexOffset;

                vertices[vertexOffset++] = face.a;
                vertices[vertexOffset++] = face.b;
                vertices[vertexOffset++] = face.c;
                vertices[vertexOffset++] = face.d;

                uvs[uvOffset++] = face.uva;
                uvs[uvOffset++] = face.uvb;
                uvs[uvOffset++] = face.uvc;
                uvs[uvOffset++] = face.uvd;

                triangles[triangleOffset++] = vertexA;
                triangles[triangleOffset++] = vertexA + 1;
                triangles[triangleOffset++] = vertexA + 2;

                triangles[triangleOffset++] = vertexA + 1;
                triangles[triangleOffset++] = vertexA + 3;
                triangles[triangleOffset++] = vertexA + 2;
            }

            // Zero any remaining triangles
            while (triangleOffset < triangles.Length) {
                triangles[triangleOffset++] = 0;
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            collider.sharedMesh = mesh;

            dirty = false;
        }
    }
}
