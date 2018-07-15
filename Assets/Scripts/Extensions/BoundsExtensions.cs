using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions {
    public static Bounds TransformBounds(this Transform transform, Bounds bounds) {
        Vector3[] corners = {
                bounds.min,
                bounds.min,
                bounds.min,
                bounds.min,

                bounds.max,
                bounds.max,
                bounds.max,
                bounds.max,
            };
        corners[1].x = bounds.max.x;
        corners[2].y = bounds.max.y;
        corners[3].z = bounds.max.z;

        corners[5].x = bounds.min.x;
        corners[6].y = bounds.min.y;
        corners[7].z = bounds.min.z;

        Bounds newBounds = new Bounds();
        for (int i = 0; i < corners.Length; ++i) {
            corners[i] = transform.TransformPoint(corners[i]);
            newBounds.Encapsulate(corners[i]);
        }

        return newBounds;
    }

    public static Mesh TransformMesh(this Transform transform, Mesh mesh) {
        Mesh newMesh = Object.Instantiate(mesh);
        Vector3[] newVertices = new Vector3[mesh.vertexCount];
        for (int i = 0; i < newVertices.Length; ++i) {
            newVertices[i] = transform.TransformPoint(mesh.vertices[i]);
        }
        newMesh.vertices = newVertices;

        return newMesh;
    }
}
